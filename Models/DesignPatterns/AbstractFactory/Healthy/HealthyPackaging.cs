using CraveCart.Models.AbstractFactory.Interfaces;

namespace CraveCart.Models.AbstractFactory.Healthy
{
    public class HealthyPackaging : IPackaging
    {
        public string GetPackagingType()
        {
            return "TakeAway Box";
        }
    }
}
