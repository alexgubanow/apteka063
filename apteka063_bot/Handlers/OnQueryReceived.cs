using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace apteka063.bot;

public partial class UpdateHandlers
{
    private static async Task OnQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var user = await dbc.User.GetUserAsync(_db, callbackQuery.From);
        if (callbackQuery.Data == "backtoMain")
        {
            await OnMessageReceived(botClient, callbackQuery.Message);
        }
        else if (callbackQuery.Data == "pills")
        {
            await OnPillsReplyReceived(botClient, callbackQuery);
        }
        else if (callbackQuery.Data.Contains("pill_") == true)
        {
            await OnPillsItemReplyReceived(botClient, callbackQuery);
        }
        else if (callbackQuery.Data == "order")
        {
            await OnOrderReplyReceived(botClient, callbackQuery);
        }
        else
        {
            await OnMessageReceived(botClient, callbackQuery.Message);
        }
    }
    private static async Task OnPillsReplyReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery, dbc.Order? order = null)
    {
        await botClient.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
        order ??= _db.Orders.Where(x => x.UserId == callbackQuery.From.Id && x.Status != dbc.OrderStatus.Closed).FirstOrDefault();
        if (order == null)
        {
            order = new() { UserId = callbackQuery.From.Id };
            await _db.Orders.AddAsync(order);
            await _db.SaveChangesAsync();
        }
        var orderPills = order.Pills?.Split(',');
        InlineKeyboardMarkup inlineKeyboard = new(new[] {
            new [] { InlineKeyboardButton.WithCallbackData("Back to main menu", "backtoMain"), },
            new [] { InlineKeyboardButton.WithCallbackData("Korvalment" + (orderPills != null && orderPills.Contains("pill_1") ? GEmojiSharp.Emoji.Emojify(" :ballot_box_with_check:") : ""), "pill_1") },
            new [] { InlineKeyboardButton.WithCallbackData("Valeriana" + (orderPills != null && orderPills.Contains("pill_2") ? GEmojiSharp.Emoji.Emojify(" :ballot_box_with_check:") : ""), "pill_2") },
            new [] { InlineKeyboardButton.WithCallbackData("Order", "order") }, });
        await botClient.SendTextMessageAsync(chatId: callbackQuery.Message.Chat.Id, text: "Here is what we have:", replyMarkup: inlineKeyboard);
    }
    private static async Task OnPillsItemReplyReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var order = _db.Orders.Where(x => x.UserId == callbackQuery.From.Id && x.Status != dbc.OrderStatus.Closed).FirstOrDefault();
        if (order == null)
        {
            order = new() { UserId = callbackQuery.From.Id };
            await _db.Orders.AddAsync(order);
            await _db.SaveChangesAsync();
        }
        var orderPillsList = order.Pills?.Split(',').ToList();
        if (orderPillsList != null)
        {
            if (orderPillsList.Contains(callbackQuery.Data))
            {
                orderPillsList.Remove(callbackQuery.Data);
            }
            else
            {
                orderPillsList.Add(callbackQuery.Data);
            }
        }
        else
        {
            orderPillsList = new() { callbackQuery.Data };
        }
        order.Pills = string.Join(',', orderPillsList);
        _db.Orders.Update(order);
        await _db.SaveChangesAsync();
        await OnPillsReplyReceived(botClient, callbackQuery, order);
    }
    private static async Task OnOrderReplyReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var order = _db.Orders.Where(x => x.UserId == callbackQuery.From.Id && x.Status != dbc.OrderStatus.Closed).FirstOrDefault();
        if (order == null)
        {
            order = new() { UserId = callbackQuery.From.Id };
            await _db.Orders.AddAsync(order);
            await _db.SaveChangesAsync();
        }
        if (order.Pills == null)
        {
            await OnPillsReplyReceived(botClient, callbackQuery, order);
        }
        string[] Scopes = { SheetsService.Scope.Spreadsheets };
        //var clientSecrets = await GoogleClientSecrets.FromFileAsync("apteka063-b1c869e99bc4.json");
        //var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(clientSecrets.Secrets, Scopes, "user", CancellationToken.None, new FileDataStore("token.json", true)).Result;
        ServiceAccountCredential credential;
        string serviceAccountEmail = "apteka063-bot@apteka063.iam.gserviceaccount.com";
        string jsonfile = "googlecreds.json";
        using (Stream stream = new FileStream(@jsonfile, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            credential = (ServiceAccountCredential)
                GoogleCredential.FromStream(stream).UnderlyingCredential;

            var initializer = new ServiceAccountCredential.Initializer(credential.Id)
            {
                User = serviceAccountEmail,
                Key = credential.Key,
                Scopes = Scopes
            };
            credential = new ServiceAccountCredential(initializer);
        }

        // Create Google Sheets API service.
        var service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "apteka063_bot",
        });
        string spreadsheetId = "1d90xhyr_zrIFTTfccrDnav5lc9nMEhnKEWpTyUYEKOg";
        try
        {
            var request = service.Spreadsheets.Values.Get(spreadsheetId, "Orders!A2:A");
            var response = await request.ExecuteAsync();
            var writePosition = -1;
            if (response.Values != null)
            {
                for (int i = 0; i < response.Values[0].Count; i++)
                {
                    if (response.Values[0][i].ToString() == order.Id.ToString())
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
            valueRange.Values = new List<IList<object>> { new List<object>() { order.Id.ToString() }, new List<object>() { callbackQuery.From.FirstName }, new List<object>() { order.Pills ?? "" } };
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
        await OnPillsReplyReceived(botClient, callbackQuery, order);
    }
}
