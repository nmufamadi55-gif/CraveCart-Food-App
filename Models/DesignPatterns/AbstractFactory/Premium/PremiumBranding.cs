using CraveCart.Models.AbstractFactory.Interfaces;

namespace CraveCart.Models.AbstractFactory.Premium
{
    public class PremiumBranding: IBranding
    {
        public string GetBrandTheme()
        {
            return "Premium Branding";
        }
    }
}
