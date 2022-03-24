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
    }
}
