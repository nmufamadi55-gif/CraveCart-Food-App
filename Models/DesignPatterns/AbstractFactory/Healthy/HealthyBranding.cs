using CraveCart.Models.AbstractFactory.Interfaces;

namespace CraveCart.Models.AbstractFactory.Healthy
{
    public class HealthyBranding :IBranding
    {
        public string GetBrandTheme()
        {
            return "Healthy  Branding";
        }
    }
}
