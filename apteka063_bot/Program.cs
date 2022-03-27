using System.Configuration;
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
        if (_db.Database.EnsureCreated() == true)
        {
            if (await Services.Gsheet.SyncPillsAsync(_db) == 0)
            {
                Console.WriteLine(Resources.Translation.DBUpdateFinished);
            }
            else
            {
                Console.WriteLine(Resources.Translation.DBUpdateFailed);
            }
        _db.Database.EnsureCreated();
        TelegramBotClient Bot = null;
        var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        if (config == null)
        {
            Console.WriteLine("failed to open app config");
            return -1;
        }
        //var tokenConfig = config.GetSection("Token");
        int tryCount = 0;
        string tokenFromArguments = "";
        if (args.Length > 0)
        {
            tokenFromArguments = args[0];
        }
        while ((config!.AppSettings.Settings["Token"] == null || Bot == null) && tryCount < 5)
        {
            tryCount++;
            string token = config!.AppSettings.Settings["Token"].Value ?? "";
            if (tokenFromArguments != "")
            {
                token = tokenFromArguments;
            }
            else
            {
                Console.WriteLine("Please enter telegram token to be used:");
                token = Console.ReadLine()!;
            }
            try
            {
                Bot = new(token);
                if (config!.AppSettings.Settings["Token"] != null)
                {
                    config!.AppSettings.Settings["Token"].Value = token;
                }
                else
                {
                    config!.AppSettings.Settings.Add(new("Token", token));
                }
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("Token");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"failed to create bot with given token:\n{token}");
                Console.WriteLine($"original error message:\n{ex.Message}");
            }
        }

        TelegramBotClient Bot = new(token);

        User me = await Bot.GetMeAsync();
        Console.Title = me.Username ?? "apteka063";

        using var cts = new CancellationTokenSource();

        // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
        ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };
        Bot!.StartReceiving(bot.UpdateHandlers.HandleUpdateAsync, bot.UpdateHandlers.HandleErrorAsync, receiverOptions, cts.Token);

        Console.WriteLine($"Start listening for @{me.Username}");
        Console.ReadKey(true);
        // Send cancellation request to stop bot
        cts.Cancel();
        return 0;
    }
}