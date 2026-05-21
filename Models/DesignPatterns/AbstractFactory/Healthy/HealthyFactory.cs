using CraveCart.Models.AbstractFactory.Interfaces;

namespace CraveCart.Models.AbstractFactory.Healthy
{
    public class HealthyFactory : IRestaurantFactory
    {
        public IMenuItem CreateMenuItem()
        {
            return new HealthyMenu();
        }

        public IPackaging CreatePackaging()
        {
            return new HealthyPackaging();
        }

        public IBranding CreateBranding()
        {
            return new HealthyBranding();
        }
    }
}
