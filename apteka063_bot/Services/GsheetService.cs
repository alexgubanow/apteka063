using apteka063.Database;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
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
        
        public async Task PostOrder(Order order, string person, string personID, string pills)
        {
            string orderID = order.Id.ToString();

            var service = GetSheetsSevice();
            try
            {
                var request = service.Spreadsheets.Values.Get(spreadsheetId, "Заказы!A2:A");
                var response = await request.ExecuteAsync();
                var writePosition = -1;
                if (response.Values != null)
                {
                    for (int i = 0; i < response.Values[0].Count; i++)
                    {
                        if (response.Values[0][i].ToString() == orderID)
                        {
                            writePosition = i + 2;
                        }
                    }
                    if (writePosition == -1)
                    {
                        writePosition = response.Values[0].Count + 2;
                    }
                }
                else
                {
                    writePosition = 2;
                }

                ValueRange valueRange = new ValueRange() { MajorDimension = "COLUMNS" };
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
                    new List<object>() { DateTime.Now.ToString("MM/dd/yyyy H:mm:ss") }, // Format depend on Google sheet
                };
                if (writePosition != -1)
                {
                    var update = service.Spreadsheets.Values.Update(valueRange, spreadsheetId, $"Заказы!A{writePosition}:J{writePosition}");
                    update.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                    UpdateValuesResponse result2 = await update.ExecuteAsync();
                }
                else
                {
                    var request1 = service.Spreadsheets.Values.Append(valueRange, spreadsheetId, $"Заказы!A{writePosition}:J{writePosition}");
                    request1.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                    var result2 = await request1.ExecuteAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
        public async Task<int> SyncPillCategoriesAsync()
        {
            var service = GetSheetsSevice();
            try
            {
                var request = service.Spreadsheets.Values.Get(spreadsheetId, "Таблетки!N2:N");
                var response = await request.ExecuteAsync();
                if (response.Values != null)
                {
                    await _db.TruncatePillCategoriesAsync();
                    List<PillCategory> itemsList = new(response.Values.Count);
                    for (int i = 0; i < response.Values.Count; i++)
                    {
                        itemsList.Add(new() { Name = response.Values[i][0].ToString() });
                    }
                    await _db.PillCategories.AddRangeAsync(itemsList);
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
            return 0;
        }
        
        public async Task<int> SyncPillsAsync()
        {
            var service = GetSheetsSevice();
            try
            {
                var request = service.Spreadsheets.Values.Get(spreadsheetId, "Таблетки!A2:B");
                var response = await request.ExecuteAsync();
                if (response.Values != null)
                {
                    await _db.TruncatePillsAsync();
                    List<Pill> itemsList = new(response.Values.Count);
                    for (int i = 0; i < response.Values.Count; i++)
                    {
                        itemsList.Add(new($"p{i}", response.Values[i]));
                    }
                    await _db.Pills.AddRangeAsync(itemsList);
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
            return 0;
        }

        public async Task<int> SyncFoodAsync()
        {
            var service = GetSheetsSevice();
            try
            {
                var request = service.Spreadsheets.Values.Get(spreadsheetId, "Еда!A2:B");
                var response = await request.ExecuteAsync();
                if (response.Values != null)
                {
                    await _db.TruncateFoodAsync();
                    List<Food> itemsList = new(response.Values.Count);
                    for (int i = 0; i < response.Values.Count; i++)
                    {
                        itemsList.Add(new($"f{i}",response.Values[i]));
                    }
                    await _db.Foods.AddRangeAsync(itemsList);
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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

        public async Task<bool> TrySyncAllTablesToDb()
        {
            var success = true;
            
            if (await SyncPillsAsync() != 0)
            {
                success = false;
                _logger.LogCritical(Resources.Translation.DBUpdateFailed);
            }

            if (await SyncFoodAsync() != 0)
            {
                success = false;
                _logger.LogCritical(Resources.Translation.DBUpdateFailed);
            }

            if (await SyncPillCategoriesAsync() != 0)
            {
                success = false;
                _logger.LogCritical(Resources.Translation.DBUpdateFailed);
            }

            _logger.LogInformation(Resources.Translation.DBUpdateFinished);
            return success;
        }
    }
}
