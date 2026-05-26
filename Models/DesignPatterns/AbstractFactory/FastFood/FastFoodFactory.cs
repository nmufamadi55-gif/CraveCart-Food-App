using CraveCart.Models.AbstractFactory.Interfaces;

namespace CraveCart.Models.AbstractFactory.FastFood
{
    public class FastFoodFactory : IRestaurantFactory
    {
        public IMenuItem CreateMenuItem()
        {
            return new FastFoodMenu();
        }

        public IPackaging CreatePackaging()
        {
            return new FastFoodPackaging();
        }

        public IBranding CreateBranding()
        {
            return new FastFoodBranding();
        }

        public object CreateMeal()
        {
            throw new NotImplementedException();
        }
    }
}
