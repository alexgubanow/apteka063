using apteka063.Database;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace apteka063.Services
{
    public class Gsheet
    {
        private readonly ILogger<Gsheet> _logger;
        private readonly Apteka063Context _db;
        public Gsheet(ILogger<Gsheet> logger, Apteka063Context db)
        {
            _logger = logger;
            _db = db;
        }
        static string serviceAccountEmail = "apteka063-bot@apteka063.iam.gserviceaccount.com";
        static string jsonfile = "googlecreds.json";
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string spreadsheetId = "1d90xhyr_zrIFTTfccrDnav5lc9nMEhnKEWpTyUYEKOg";

        public async Task PostOrder(Order order, string person, string personID, string pills, CancellationToken cts = default)
        {
            string orderID = order.Id.ToString();

            var service = GetSheetsSevice();
            try
            {
                var request = service.Spreadsheets.Values.Get(spreadsheetId, "Заказы!A2:A");
                var response = await request.ExecuteAsync(cts);
                var writePosition = -1;
                if (response.Values != null)
                {
                    int orderToUpdateIDX = response.Values[0].IndexOf(orderID);
                    writePosition = orderToUpdateIDX != -1 ? orderToUpdateIDX : response.Values.Count + 2;
                }
                else
                {
                    writePosition = 2;
                }
                writePosition = writePosition > 0 ? writePosition : 2;
                ValueRange valueRange = new() { MajorDimension = "COLUMNS" };
                valueRange.Values = new List<IList<object>> {
                    new List<object>() { orderID },
                    new List<object>() { "not supported" },
                    new List<object>() { person },
                    new List<object>() { personID != null ? $"https://t.me/{personID}" : "не найдено" },
                    new List<object>() { pills },
                    new List<object>() { "not supported" },
                    new List<object>() { order.ContactPhone },
                    new List<object>() { order.DeliveryAddress },
                    new List<object>() { "not supported" },
                    new List<object>() { DateTime.Now.ToString("MM/dd/yyyy H:mm:ss") },
                    new List<object>() { $"=IF(J{writePosition},HOUR(NOW()-J{writePosition}) + DAYS(NOW(), J{writePosition}) * 24, -1)" }, // Format depend on Google sheet
                };
                var update = service.Spreadsheets.Values.Update(valueRange, spreadsheetId, $"Заказы!A{writePosition}:K{writePosition}");
                update.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                UpdateValuesResponse result2 = await update.ExecuteAsync(cts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
        public async Task UpdateFreezedValues(CancellationToken cts = default)
        {
            var service = GetSheetsSevice();
            try
            {
                ValueRange valueRange = new() { MajorDimension = "ROWS" };
                valueRange.Values = new List<IList<object>>();
                foreach (var pill in _db.ItemsToOrder.Where(x => x.Id.StartsWith('p')))
                {
                    valueRange.Values.Add(new List<object>() { pill.FreezedAmout });
                }
                var update = service.Spreadsheets.Values.Update(valueRange, spreadsheetId, "Таблетки!C2:C");
                update.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                UpdateValuesResponse result = await update.ExecuteAsync(cts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private async Task<int> SyncItemsCategoriesAsync(CancellationToken cts = default)
        {
            var service = GetSheetsSevice();
            try
            {
                await _db.ClearItemsCategoriesAsync(cts);
                var request = service.Spreadsheets.Values.Get(spreadsheetId, "Таблетки!N2:N");
                var response = await request.ExecuteAsync(cts);
                if (response.Values != null)
                {
                    for (int i = 0; i < response.Values.Count; i++)
                    {
                        await _db.ItemsCategories.AddAsync(new() { Id = $"pc{i}", Name = response.Values[i][0].ToString()!, Section = "pills" }, cts);
                    }
                }
                request = service.Spreadsheets.Values.Get(spreadsheetId, "Гуманитарка!N2:N");
                response = await request.ExecuteAsync(cts);
                if (response.Values != null)
                {
                    for (int i = 0; i < response.Values.Count; i++)
                    {
                        await _db.ItemsCategories.AddAsync(new() { Id = $"fc{i}", Name = response.Values[i][0].ToString()!, Section = "humaid" }, cts);
                    }
                    await _db.SaveChangesAsync(cts);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,ex.Message);
                return -1;
            }
            return 0;
        }
        
        private async Task<int> SyncItemsToOrderAsync(CancellationToken cts = default)
        {
            var service = GetSheetsSevice();
            try
            {
                await _db.ClearItemsToOrderAsync(cts);
                var request = service.Spreadsheets.Values.Get(spreadsheetId, "Таблетки!A2:C");
                var response = await request.ExecuteAsync(cts);
                if (response.Values != null)
                {
                    for (int i = 0; i < response.Values.Count; i++)
                    {
                        var category = await _db.ItemsCategories.FirstOrDefaultAsync(x => x.Name == response.Values[i][1].ToString()!, cts);
                        var categoryId = category != null ? category.Id : "p0";
                        await _db.ItemsToOrder.AddAsync(new($"p{i}", response.Values[i][0].ToString()!, categoryId, int.Parse(response.Values[i][2].ToString()!)), cts);
                    }
                }
                request = service.Spreadsheets.Values.Get(spreadsheetId, "Гуманитарка!A2:C");
                response = await request.ExecuteAsync(cts);
                if (response.Values != null)
                {
                    for (int i = 0; i < response.Values.Count; i++)
                    {
                        var category = await _db.ItemsCategories.FirstOrDefaultAsync(x => x.Name == response.Values[i][1].ToString()!, cts);
                        var categoryId = category != null ? category.Id : "f0";
                        await _db.ItemsToOrder.AddAsync(new($"f{i}", response.Values[i][0].ToString()!, categoryId, int.Parse(response.Values[i][2].ToString()!)), cts);
                    }
                }
                await _db.SaveChangesAsync(cts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return -1;
            }
            return 0;
        }

        private static SheetsService GetSheetsSevice()
        {
            ServiceAccountCredential credential;
            using (Stream stream = new FileStream(@jsonfile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                credential = (ServiceAccountCredential)GoogleCredential.FromStream(stream).UnderlyingCredential;
                var initializer = new ServiceAccountCredential.Initializer(credential.Id)
                {
                    User = serviceAccountEmail,
                    Key = credential.Key,
                    Scopes = Scopes
                };
                credential = new ServiceAccountCredential(initializer);
            }
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "apteka063_bot",
            });
            return service;
        }

        public async Task<bool> SyncAllTablesToDb(CancellationToken cts = default)
        {
            var success = true;
            if (await SyncItemsCategoriesAsync(cts) != 0)
            {
                success = false;
                _logger.LogCritical(Resources.Translation.DBUpdateFailed);
            }
            if (await SyncItemsToOrderAsync(cts) != 0)
            {
                success = false;
                _logger.LogCritical(Resources.Translation.DBUpdateFailed);
            }
            _logger.LogInformation(Resources.Translation.DBUpdateFinished);
            return success;
        }
    }
}
