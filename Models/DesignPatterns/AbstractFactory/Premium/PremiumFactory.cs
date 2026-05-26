using System.Reflection.Metadata.Ecma335;
using CraveCart.Models.AbstractFactory.Interfaces;

namespace CraveCart.Models.AbstractFactory.Premium
{
    public class PremiumFactory: IRestaurantFactory
    {
        public IMenuItem CreateMenuItem()
        {
            return new PremiumMenu();
        }

        public IPackaging CreatePackaging()
        {
            return new PremiumPackaging();
        }

        public IBranding CreateBranding()
        {
            return new PremiumBranding();
        }

        public object CreateMeal()
        {
            throw new NotImplementedException();
        }
    }
}
