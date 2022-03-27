using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apteka063.Services
{
    public class Gsheet
    {
        private readonly ILogger<Gsheet> _logger;
        private readonly dbc.Apteka063Context _db;
        public Gsheet(ILogger<Gsheet> logger, dbc.Apteka063Context db)
        {
            _logger = logger;
            _db = db;
        }
        static string serviceAccountEmail = "apteka063-bot@apteka063.iam.gserviceaccount.com";
        static string jsonfile = "googlecreds.json";
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string spreadsheetId = "1d90xhyr_zrIFTTfccrDnav5lc9nMEhnKEWpTyUYEKOg";
        public static async Task PostOrder(string orderID, string person, string personID, string pills)
        {
            var service = GetSheetsSevice();
            try
            {
                var request = service.Spreadsheets.Values.Get(spreadsheetId, "Orders!A2:A");
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
                    new List<object>() { person },
                    new List<object>() { personID != null ? $"https://t.me/{personID}" : "не найдено" },
                    new List<object>() { pills },
                    new List<object>() { "not supported" },
                    new List<object>() { "not supported" },
                    new List<object>() { "not supported" },
                    new List<object>() { DateTime.Now.ToString("MM/dd/yyyy H:mm:ss") }, // Format depend on Google sheet
                };
                if (writePosition != -1)
                {
                    var update = service.Spreadsheets.Values.Update(valueRange, spreadsheetId, $"Orders!A{writePosition}:H{writePosition}");
                    update.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                    UpdateValuesResponse result2 = await update.ExecuteAsync();
                }
                else
                {
                    var request1 = service.Spreadsheets.Values.Append(valueRange, spreadsheetId, $"Orders!A{writePosition}:H{writePosition}");
                    request1.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                    var result2 = await request1.ExecuteAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public readonly static Dictionary<string, dbc.PillCategories> pillCategoriesMap = new()
        {
            { "Сердце", dbc.PillCategories.Heart },
            { "Желудок", dbc.PillCategories.Stomach },
            { "Обезболивающее", dbc.PillCategories.Painkiller },
            { "Температура", dbc.PillCategories.Fever },
            { "Детское", dbc.PillCategories.Child },
            { "Женское", dbc.PillCategories.Women },
            { "Другое", dbc.PillCategories.Other }
        };
        public async Task<int> SyncPillsAsync()
        {
            var service = GetSheetsSevice();
            try
            {
                var request = service.Spreadsheets.Values.Get(spreadsheetId, "Pills!A2:C");
                var response = await request.ExecuteAsync();
                if (response.Values != null)
                {
                    foreach (var sheetRow in response.Values)
                    {
                        int pillID = int.Parse(sheetRow[0].ToString()!);
                        var pill = _db.Pills!.Where(x => x.Id == pillID).FirstOrDefault();
                        if (pill == null)
                        {
                            pill = new() { Id = pillID, Name = sheetRow[1].ToString()!, PillCategory = pillCategoriesMap[sheetRow[2].ToString()!] };
                            await _db.Pills!.AddAsync(pill);
                        }
                        else
                        {
                            pill.Id = pillID;
                            pill.Name = sheetRow[1].ToString()!;
                            pill.PillCategory = pillCategoriesMap[sheetRow[2].ToString()!];
                            _db.Pills!.Update(pill);
                        }
                        await _db.SaveChangesAsync();
                    }
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
    }
}
