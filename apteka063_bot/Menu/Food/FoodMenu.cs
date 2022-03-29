using Microsoft.Extensions.Logging;

namespace apteka063.menu;

public partial class FoodMenu
{
    private readonly ILogger<FoodMenu> _logger;
    private readonly dbc.Apteka063Context _db;
    public FoodMenu(ILogger<FoodMenu> logger, dbc.Apteka063Context db)
    {
        _logger = logger;
        _db = db;
    }
}
