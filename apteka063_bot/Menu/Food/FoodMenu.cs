using Microsoft.Extensions.Logging;

namespace apteka063.menu;

public partial class FoodMenu
{
    private readonly ILogger<FoodMenu> _logger;
    private readonly dbc.Apteka063Context _db;
    private readonly Order _order;
    public FoodMenu(ILogger<FoodMenu> logger, dbc.Apteka063Context db,Order order)
    {
        _logger = logger;
        _db = db;
        _order = order;
    }
}
