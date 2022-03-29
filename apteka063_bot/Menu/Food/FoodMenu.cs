using apteka063.Database;
using Microsoft.Extensions.Logging;

namespace apteka063.Menu.Food;

public partial class FoodMenu
{
    private readonly ILogger<FoodMenu> _logger;
    private readonly Apteka063Context _db;
    public FoodMenu(ILogger<FoodMenu> logger, Apteka063Context db)
    {
        _logger = logger;
        _db = db;
    }
}
