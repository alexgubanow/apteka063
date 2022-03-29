using Microsoft.Extensions.Logging;

namespace apteka063.menu;

public partial class PillsMenu
{
    private readonly ILogger<PillsMenu> _logger;
    private readonly dbc.Apteka063Context _db;
    public PillsMenu(ILogger<PillsMenu> logger, dbc.Apteka063Context db)
    {
        _logger = logger;
        _db = db;
    }
}
