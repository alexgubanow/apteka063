using apteka063.Menu.Food;
using apteka063.Menu.Pills;

namespace apteka063.Menu;

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
