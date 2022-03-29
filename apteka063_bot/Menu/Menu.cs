using Microsoft.Extensions.Logging;

namespace apteka063.menu;

public class Menu
{
    public readonly PillsMenu Pills;
    public readonly FoodMenu Food;
    public Menu(PillsMenu pills, FoodMenu food)
    {
        Pills = pills;
        Food = food;
    }
}
