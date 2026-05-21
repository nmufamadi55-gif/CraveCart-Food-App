using System.Collections.Generic;

namespace CraveCart.Models.DesignPatterns
{
   
    // FACTORY METHOD PATTERN
    
    public interface IMeal
    {
        int Id { get; }
        string Name { get; }
        decimal Price { get; }
        string Type { get; }
        string Icon { get; }
        string Description { get; }
    }

    public class BurgerMeal : IMeal
    {
        public int Id => 1;
        public string Name => "Classic Beef Burger";
        public decimal Price => 55.00m;
        public string Type => "burger";
        public string Icon => "🍔";
        public string Description => "Juicy beef patty with lettuce and tomato";
    }

    public class PizzaMeal : IMeal
    {
        public int Id => 2;
        public string Name => "Margherita Pizza";
        public decimal Price => 90.00m;
        public string Type => "pizza";
        public string Icon => "🍕";
        public string Description => "Fresh mozzarella and basil";
    }

    public class PastaMeal : IMeal
    {
        public int Id => 3;
        public string Name => "Fettuccine Alfredo";
        public decimal Price => 125.00m;
        public string Type => "pasta";
        public string Icon => "🍝";
        public string Description => "Creamy parmesan sauce";
    }

    public class ChickenMeal : IMeal
    {
        public int Id => 4;
        public string Name => "Grilled Chicken";
        public decimal Price => 75.00m;
        public string Type => "chicken";
        public string Icon => "🍗";
        public string Description => "Grilled chicken breast with vegetables";
    }

    public static class MealFactory
    {
        public static IMeal CreateMeal(string type)
        {
            return type?.ToLower() switch
            {
                "burger" => new BurgerMeal(),
                "pizza" => new PizzaMeal(),
                "pasta" => new PastaMeal(),
                "chicken" => new ChickenMeal(),
                _ => new BurgerMeal()
            };
        }
    }
}