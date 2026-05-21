using System.Collections.Generic;

namespace CraveCart.Models.DesignPatterns
{
    
    // DECORATOR PATTERN
 
    public abstract class MealDecorator : IMeal
    {
        protected IMeal _meal;
        protected List<string> _extras;

        public MealDecorator(IMeal meal)
        {
            _meal = meal;
            _extras = new List<string>();
        }

        public virtual int Id => _meal.Id;
        public virtual string Name => _meal.Name;
        public virtual decimal Price => _meal.Price;
        public virtual string Type => _meal.Type;
        public virtual string Icon => _meal.Icon;
        public virtual string Description => _meal.Description;

        public List<string> GetExtras() => _extras;
    }

    public class ExtraCheeseDecorator : MealDecorator
    {
        public ExtraCheeseDecorator(IMeal meal) : base(meal)
        {
            _extras.Add("Extra Cheese");
        }
        public override string Name => _meal.Name;
        public override decimal Price => _meal.Price + 10m;
    }

    public class ExtraBaconDecorator : MealDecorator
    {
        public ExtraBaconDecorator(IMeal meal) : base(meal)
        {
            _extras.Add("Bacon");
        }
        public override string Name => _meal.Name;
        public override decimal Price => _meal.Price + 15m;
    }

    public static class DecoratorHelper
    {
        public static IMeal ApplyExtras(IMeal meal, List<string> extras)
        {
            var result = meal;
            foreach (var extra in extras)
            {
                result = extra switch
                {
                    "Extra Cheese" => new ExtraCheeseDecorator(result),
                    "Bacon" => new ExtraBaconDecorator(result),
                    _ => result
                };
            }
            return result;
        }
    }
}