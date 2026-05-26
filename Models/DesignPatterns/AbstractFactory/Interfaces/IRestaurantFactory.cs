namespace CraveCart.Models.AbstractFactory.Interfaces
{
    public interface IRestaurantFactory
    {
        IMenuItem CreateMenuItem();
        IPackaging CreatePackaging();
        IBranding CreateBranding();
        object CreateMeal();
    }
}
