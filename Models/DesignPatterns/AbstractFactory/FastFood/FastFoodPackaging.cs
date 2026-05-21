using CraveCart.Models.AbstractFactory.Interfaces;

namespace CraveCart.Models.AbstractFactory.FastFood
{
    public class FastFoodPackaging : IPackaging
    {
        public string GetPackagingType()
        {
            return "TakeAway Box";
        }
    }
}