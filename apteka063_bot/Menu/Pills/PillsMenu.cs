using apteka063.Database;
using Microsoft.Extensions.Logging;

namespace apteka063.Menu.Pills;

public partial class PillsMenu
{
    private readonly ILogger<PillsMenu> _logger;
    private readonly Apteka063Context _db;
    public PillsMenu(ILogger<PillsMenu> logger, Apteka063Context db)
    {
        _logger = logger;
        _db = db;
    }
}
