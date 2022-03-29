using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<apteka063.Worker>();
        services.AddTransient<apteka063.Services.Gsheet>();
        services.AddTransient<apteka063.bot.UpdateHandlers>();
        services.AddTransient<apteka063.dbc.Apteka063Context>();
        services.AddTransient<apteka063.menu.Menu>();
        services.AddTransient<apteka063.menu.PillsMenu>();
        services.AddTransient<apteka063.menu.FoodMenu>();
        services.AddTransient<apteka063.menu.OrderButton>();
    })
    .ConfigureLogging((_, logging) =>
    {
        logging.ClearProviders();
        logging.AddSimpleConsole(options => options.IncludeScopes = true);
        logging.AddEventLog();
    })
    .Build();

await host.RunAsync();