using CraveCart.Models.AbstractFactory.Interfaces;

namespace CraveCart.Models.AbstractFactory.Premium
{
    public class PremiumMenu :IMenuItem
    {
        public string GetMenuDescription()
        {
            return "Premium Branding";
        }
    }
}
