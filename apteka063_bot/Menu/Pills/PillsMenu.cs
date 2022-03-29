using Microsoft.Extensions.Logging;

namespace apteka063.menu;

public partial class PillsMenu
{
    private readonly ILogger<PillsMenu> _logger;
    private readonly dbc.Apteka063Context _db;
    private readonly Order _order;
    public PillsMenu(ILogger<PillsMenu> logger, dbc.Apteka063Context db, Order order)
    {
        _logger = logger;
        _db = db;
        _order = order;
    }
}
