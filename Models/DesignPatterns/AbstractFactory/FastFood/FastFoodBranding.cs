using CraveCart.Models.AbstractFactory.Interfaces;

namespace CraveCart.Models.AbstractFactory.FastFood
{
    public class FastFoodBranding : IBranding
    {
        public string GetBrandTheme()
        {
            return "FastFood Branding";
        }
    }
}
