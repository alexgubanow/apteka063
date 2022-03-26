﻿using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apteka063.Services
{
    public class Gsheet
    {
        static string serviceAccountEmail = "apteka063-bot@apteka063.iam.gserviceaccount.com";
        static string jsonfile = "googlecreds.json";
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string spreadsheetId = "1d90xhyr_zrIFTTfccrDnav5lc9nMEhnKEWpTyUYEKOg";
        public static async Task PostOrder(string orderID, string person, string pills)
        {
            SheetsService service = GetSheets();
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
                valueRange.Values = new List<IList<object>> { new List<object>() { orderID }, new List<object>() { person }, new List<object>() { pills } };
                if (writePosition != -1)
                {
                    var update = service.Spreadsheets.Values.Update(valueRange, spreadsheetId, $"Orders!A{writePosition}:C{writePosition}");
                    update.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
                    UpdateValuesResponse result2 = await update.ExecuteAsync();
                }
                else
                {
                    var request1 = service.Spreadsheets.Values.Append(valueRange, spreadsheetId, $"Orders!A{writePosition}:C{writePosition}");
                    request1.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
                    var result2 = await request1.ExecuteAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public static async Task<int> SyncPills(dbc.Apteka063Context db)
        {
            SheetsService service = GetSheets();
            try
            {
                var request = service.Spreadsheets.Values.Get(spreadsheetId, "Pills!A2:B");
                var response = await request.ExecuteAsync();
                if (response.Values != null)
                {
                    foreach (var sheetRow in response.Values)
                    {
                        int pillID = int.Parse(sheetRow[0].ToString());
                        var pill = db.Pills.Where(x => x.Id == pillID).FirstOrDefault();
                        if (pill == null)
                        {
                            pill = new() { Id = pillID, Name = sheetRow[1].ToString() };
                            await db.Pills.AddAsync(pill);
                        }
                        else
                        {
                            pill.Id = pillID;
                            pill.Name = sheetRow[1].ToString();
                            db.Pills.Update(pill);
                        }
                        await db.SaveChangesAsync();
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

        private static SheetsService GetSheets()
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