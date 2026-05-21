using CraveCart.Models.AbstractFactory.Interfaces;

namespace CraveCart.Models.AbstractFactory.FastFood
{
    public class FastFoodMenu : IMenuItem
    {
        public string GetMenuDescription()
        {
            return "Pizza Combo";
        }
    }
}
