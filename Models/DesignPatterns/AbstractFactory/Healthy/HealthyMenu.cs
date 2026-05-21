using CraveCart.Models.AbstractFactory.Interfaces;

namespace CraveCart.Models.AbstractFactory.Healthy
{
    public class HealthyMenu : IMenuItem
    {
        public string GetMenuDescription()
        {
            return "Salad & Fruit Combo";
        }
    }
}
