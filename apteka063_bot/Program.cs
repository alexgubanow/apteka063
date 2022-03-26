using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace apteka063;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        using dbc.Apteka063Context _db = new();
        _db.Database.EnsureCreated();
        string token = "5254732281:AAF76_UiH2dpF6AU40JvOzb06TSCMO8Qw-4";
        //if (args.Length > 0)
        //{
        //    token = args[0];
        //}
        //else
        //{
        //    Console.WriteLine("Please enter token");
        //    token = Console.ReadLine() ?? "";
        //}
        TelegramBotClient Bot = new(token);

        User me = await Bot.GetMeAsync();
        Console.Title = me.Username ?? "apteka063";

        using var cts = new CancellationTokenSource();

        // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
        ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };
        Bot.StartReceiving(bot.UpdateHandlers.HandleUpdateAsync, bot.UpdateHandlers.HandleErrorAsync, receiverOptions, cts.Token);

        Console.WriteLine($"Start listening for @{me.Username}");
        while (true)
        {
        }
        // Send cancellation request to stop bot
        cts.Cancel();
        return 0;
    }
}