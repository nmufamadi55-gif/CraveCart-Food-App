using CraveCart.Models.AbstractFactory.Interfaces;

namespace CraveCart.Models.AbstractFactory.Premium
{
    public class PremiumPackaging :IPackaging
    {
        public string GetPackagingType()
        {
            return "Premium Packaging";
        }
    }
}
