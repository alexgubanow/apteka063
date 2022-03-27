using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    private readonly IHostApplicationLifetime _lifetime;
    private readonly bot.UpdateHandlers _handlers;
    readonly dbc.Apteka063Context _db;
    readonly Services.Gsheet _gsheet;
    public Worker(ILogger<Worker> logger, bot.UpdateHandlers handlers, dbc.Apteka063Context db, Services.Gsheet gsheet, IHostApplicationLifetime lifetime)
    {
        _handlers = handlers;
        _lifetime = lifetime;
        _logger = logger;
        _db = db;
        _gsheet = gsheet;
    }

    protected override async Task<int> ExecuteAsync(CancellationToken stoppingToken)
    {
        TelegramBotClient? Bot = null;
        var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        if (config == null)
        {
            _logger.LogError("failed to open app config");
            _lifetime.StopApplication();
            return -1;
        }
        int tryCount = 0;
        while ((config!.AppSettings.Settings["Token"] == null || Bot == null) && tryCount < 5)
        {
            tryCount++;
            string token = config!.AppSettings.Settings["Token"].Value ?? "";
            if (token == "")
            {
                _logger.LogInformation("Please enter telegram token to be used:");
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
                _logger.LogError($"failed to create bot with given token:\n{token}\noriginal error message:\n{ex.Message}");
            }
        }
        if (Bot == null)
        {
            _logger.LogError("Failed to initialize. Exiting.");
            _lifetime.StopApplication();
            return -2;
        }
        if (_db.Database.EnsureCreated() == true)
        {
            if (await _gsheet.SyncPillsAsync() == 0)
            {
                _logger.LogCritical(Resources.Translation.DBUpdateFailed);
            }

            if (await _gsheet.SyncFoodAsync() != 0)
            {
                _logger.LogCritical(Resources.Translation.DBUpdateFailed);
            }

            _logger.LogInformation(Resources.Translation.DBUpdateFinished);
        }
        Bot!.StartReceiving(_handlers.HandleUpdateAsync, _handlers.HandleErrorAsync, new ReceiverOptions() { AllowedUpdates = { } }, stoppingToken);

        User me = await Bot!.GetMeAsync(cancellationToken: stoppingToken);
        _logger.LogInformation($"Started listening for @{me.Username}");

        while (!stoppingToken.IsCancellationRequested)
        {
            //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            //await Task.Delay(1000, stoppingToken);
        }
        return 0;
    }
}