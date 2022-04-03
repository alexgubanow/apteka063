using apteka063.Database;
using apteka063.Extensions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Configuration;
using System.Globalization;
using User = apteka063.Database.User;

namespace apteka063.Services
{
    public class Gsheet
    {
        private readonly ILogger<Gsheet> _logger;
        private readonly Apteka063Context _db;
        private readonly string spreadsheetId = "";
        public Gsheet(ILogger<Gsheet> logger, Apteka063Context db)
        {
            _logger = logger;
            _db = db;
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config == null)
            {
                _logger.LogCritical("failed to open app config");
            }
            else
            {
                int tryCount = 0;
                while ((config.AppSettings.Settings["spreadsheetId"] == null) && tryCount < 5)
                {
                    tryCount++;
                    string spreadsheetId = config.AppSettings.Settings["spreadsheetId"]?.Value ?? "";
                    if (spreadsheetId == "")
                    {
                        _logger.LogInformation("Please enter spreadsheetId to be used:");
                        spreadsheetId = Console.ReadLine()!;
                    }
                    if (IsSheetIdValid(_logger, spreadsheetId) == true)
                    {
                        if (config.AppSettings.Settings["spreadsheetId"] != null)
                        {
                            config.AppSettings.Settings["spreadsheetId"].Value = spreadsheetId;
                        }
                        else
                        {
                            config.AppSettings.Settings.Add(new("spreadsheetId", spreadsheetId));
                        }
                        config.Save(ConfigurationSaveMode.Modified);
                        ConfigurationManager.RefreshSection("spreadsheetId");
                    }
                }
                config.Save(ConfigurationSaveMode.Modified);
            }
            spreadsheetId = config?.AppSettings.Settings["spreadsheetId"].Value ?? "";
        }
        static string serviceAccountEmail = "apteka063-bot@apteka063.iam.gserviceaccount.com";
        static string jsonfile = "googlecreds.json";
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        public static bool IsSheetIdValid(ILogger logger, string sheetId)
        {
            var service = GetSheetsSevice();
            try
            {
                var request = service.Spreadsheets.Values.Get(sheetId, "Таблетки!A2:A");
                var response = request.Execute();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
            return false;
        }
        public async Task PostOrder(Order order, User user, string pills, CancellationToken cts = default)
        {
            var currentLocale = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("ru");
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
                var userContact = user.Username != "" ? $"https://t.me/{user.Username}" : user.PhoneNumber != "" ? user.PhoneNumber : $"https://t.me/@id{user.Id}";
                ValueRange valueRange = new() { MajorDimension = "COLUMNS" };
                valueRange.Values = new List<IList<object>> {
                    new List<object>() { orderID },
                    new List<object>() { TranslationConverter.ToLocaleString(order.Status) },
                    new List<object>() { user.FirstName + ' ' + user.LastName },
                    new List<object>() { userContact },
                    new List<object>() { pills },
                    new List<object>() { order.ContactName },
                    new List<object>() { order.ContactPhone },
                    new List<object>() { order.DeliveryAddress },
                    new List<object>() { TranslationConverter.ToLocaleString(order.OrderType) },
                    new List<object>() { order.CreationDateTime.ToString("MM/dd/yyyy H:mm:ss") },
                    new List<object>() { $"=NOW()-J{writePosition}" }, // Format depend on Google sheet
                    new List<object>() { order.OrderComment },
                };
                var update = service.Spreadsheets.Values.Update(valueRange, spreadsheetId, $"Заказы!A{writePosition}:L{writePosition}");
                update.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                UpdateValuesResponse result2 = await update.ExecuteAsync(cts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            Thread.CurrentThread.CurrentUICulture = currentLocale;
        }
        public async Task UpdateFreezedValues(CancellationToken cts = default)
        {
            var service = GetSheetsSevice();
            try
            {
                ValueRange valueRange = new() { MajorDimension = "ROWS" };
                valueRange.Values = new List<IList<object>>();
                var pillsCategories = _db.ItemsCategories.Where(x => x.OrderType == OrderType.Pills).Select(x => x.Id);
                foreach (var pill in _db.ItemsToOrder.Where(x => pillsCategories.Contains(x.CategoryId)))
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
        private async Task<int> SyncSettingsAsync(CancellationToken cts = default)
        {
            var service = GetSheetsSevice();
            try
            {
                foreach (var item in _db.UserSettings)
                {
                    _db.UserSettings.Remove(item);
                }
                await _db.SaveChangesAsync(cts);
                var request = service.Spreadsheets.Values.Get(spreadsheetId, "Настройки!A:B");
                var response = await request.ExecuteAsync(cts);
                if (response.Values != null)
                {
                    for (int i = 0; i < response.Values.Count; i++)
                    {
                        var settingName = response.Values[i][0]?.ToString() ?? $"s{i}";
                        var settingValue = response.Values[i].Count > 1 ? response.Values[i][1]?.ToString() ?? "" : "";
                        i++;
                        while (i < response.Values.Count && (response.Values[i].Count > 1 ? response.Values[i][0].ToString() : "") == "")
                        {
                            settingValue += '\n';
                            settingValue += response.Values[i].Count > 1 ? response.Values[i][1].ToString() : "";
                            i++;
                        }
                        i--;
                        await _db.UserSettings.AddAsync(new(settingName, settingValue), cts);
                    }
                    await _db.SaveChangesAsync(cts);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return -1;
            }
            return 0;
        }
        private async Task<int> SyncItemsCategoriesAsync(CancellationToken cts = default)
        {
            var service = GetSheetsSevice();
            try
            {
                foreach (var item in _db.ItemsCategories)
                {
                    _db.ItemsCategories.Remove(item);
                }
                await _db.SaveChangesAsync(cts);
                var request = service.Spreadsheets.Values.Get(spreadsheetId, "Таблетки!N2:N");
                var response = await request.ExecuteAsync(cts);
                if (response.Values != null)
                {
                    for (int i = 0; i < response.Values.Count; i++)
                    {
                        await _db.ItemsCategories.AddAsync(new(response.Values[i][0].ToString()!, OrderType.Pills), cts);
                    }
                }
                request = service.Spreadsheets.Values.Get(spreadsheetId, "Гуманитарка!N2:N");
                response = await request.ExecuteAsync(cts);
                if (response.Values != null)
                {
                    for (int i = 0; i < response.Values.Count; i++)
                    {
                        await _db.ItemsCategories.AddAsync(new(response.Values[i][0].ToString()!, OrderType.Humaid), cts);
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
        private async Task<int> SyncItemsToOrderAsync(CancellationToken cts = default)
        {
            var service = GetSheetsSevice();
            try
            {
                foreach (var item in _db.ItemsToOrder)
                {
                    _db.ItemsToOrder.Remove(item);
                }
                await _db.SaveChangesAsync(cts);
                var request = service.Spreadsheets.Values.Get(spreadsheetId, "Таблетки!A2:C");
                var response = await request.ExecuteAsync(cts);
                if (response.Values != null)
                {
                    for (int i = 0; i < response.Values.Count; i++)
                    {
                        var category = await _db.ItemsCategories.FirstOrDefaultAsync(x => x.Name == response.Values[i][1].ToString()!, cts);
                        var categoryId = category != null ? category.Id : 0;
                        await _db.ItemsToOrder.AddAsync(new(response.Values[i][0].ToString()!, categoryId, int.Parse(response.Values[i][2].ToString()!)), cts);
                    }
                }
                request = service.Spreadsheets.Values.Get(spreadsheetId, "Гуманитарка!A2:C");
                response = await request.ExecuteAsync(cts);
                if (response.Values != null)
                {
                    for (int i = 0; i < response.Values.Count; i++)
                    {
                        var category = await _db.ItemsCategories.FirstOrDefaultAsync(x => x.Name == response.Values[i][1].ToString()!, cts);
                        var categoryId = category != null ? category.Id : 0;
                        await _db.ItemsToOrder.AddAsync(new(response.Values[i][0].ToString()!, categoryId, int.Parse(response.Values[i][2].ToString()!)), cts);
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
            if (await SyncSettingsAsync(cts) != 0)
            {
                success = false;
                _logger.LogCritical(Resources.Translation.DBUpdateFailed);
            }
            _logger.LogInformation(Resources.Translation.DBUpdateFinished);
            return success;
        }
    }
}
