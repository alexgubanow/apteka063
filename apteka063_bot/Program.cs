using apteka063.Database;
using apteka063.Handlers;
using apteka063.Menu;
using apteka063.Menu.OrderButton;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<apteka063.Worker>();
        services.AddTransient<apteka063.Services.Gsheet>();
        services.AddTransient<UpdateHandlers>();
        services.AddDbContext<Apteka063Context>();
        services.AddTransient<Menu>();
        services.AddTransient<MyOrders>();
        services.AddTransient<OrderButton>();
    })
    .ConfigureLogging((_, logging) =>
    {
        logging.ClearProviders();
        logging.AddSimpleConsole(options => options.IncludeScopes = true);
    })
    .Build();

await host.RunAsync();