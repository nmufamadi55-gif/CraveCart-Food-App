using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CraveCart.Models;
using CraveCart.Models.AbstractFactory.FastFood;
using CraveCart.Models.AbstractFactory.Premium;
using CraveCart.Models.AbstractFactory.Healthy;
using CraveCart.Models.AbstractFactory.Interfaces;
using CraveCart.Models.DesignPatterns;
using CraveCart.Services;

namespace CraveCart.Controllers
{
    public class HomeController : Controller
    {
        private static List<CartItem> _cart = new List<CartItem>();
        private static List<Restaurant> _restaurants;
        private static int _loyaltyPoints = 500;
        private static List<string> _usedPromoCodes = new List<string>();
        private static SaveLoadService _saveLoadService = new SaveLoadService();
        private static DriverTrackingService _driverService = new DriverTrackingService();
        private static List<int> _pointsEarnedHistory = new List<int>();
        private static List<int> _pointsRedeemedHistory = new List<int>();

        public HomeController()
        {
            InitializeRestaurants();
        }

        private void InitializeRestaurants()
        {
            if (_restaurants != null) return;
            _restaurants = new List<Restaurant>();

            // Customizations per food type
            List<Customization> GetBurgerExtras()
            {
                return new List<Customization>
                {
                    new Customization { Name = "Extra Cheese", Price = 10, Icon = "/images/icons/cheese.png" },
                    new Customization { Name = "Bacon", Price = 15, Icon = "/images/icons/bacon.png" },
                    new Customization { Name = "Patty", Price = 20, Icon = "/images/icons/patty.png" },
                    new Customization { Name = "Pickles", Price = 5, Icon = "/images/icons/pickles.png" }
                };
            }

            List<Customization> GetPizzaExtras()
            {
                return new List<Customization>
                {
                    new Customization { Name = "Extra Cheese", Price = 10, Icon = "/images/icons/cheese.png" },
                    new Customization { Name = "Pepperoni", Price = 12, Icon = "/images/icons/pepperoni.png" },
                    new Customization { Name = "Mushrooms", Price = 8, Icon = "/images/icons/mushrooms.png" },
                    new Customization { Name = "PineApple", Price = 8, Icon = "/images/icons/pineapple.png" }
                };
            }

            List<Customization> GetPastaExtras()
            {
                return new List<Customization>
                {
                    new Customization { Name = "Extra Parmesan", Price = 8, Icon = "/images/icons/cheese.png" },
                    new Customization { Name = "Grilled Chicken", Price = 15, Icon = "/images/icons/chicken.png" },
                    new Customization { Name = "Bread", Price = 15, Icon = "/images/icons/bread.png" }
                };
            }

            List<Customization> GetSaladExtras()
            {
                return new List<Customization>
                {
                    new Customization { Name = "Extra Avocado", Price = 12, Icon = "/images/icons/avocado.png" },
                    new Customization { Name = "Grilled Chicken", Price = 15, Icon = "/images/icons/chicken.png" },
                    new Customization { Name = "Boiled eggs", Price = 9, Icon = "/images/icons/egg.png" },
                    new Customization { Name = "Cucumber", Price = 15, Icon = "/images/icons/cucumber.png" }
                };
            }

            List<Customization> GetChickenExtras()
            {
                return new List<Customization>
                {
                    new Customization { Name = "Extra Sauce", Price = 5, Icon = "/images/icons/sauce.png" },
                    new Customization { Name = "Large Portion", Price = 10, Icon = "/images/icons/portion.png" }
                };
            }

            List<Customization> GetSushiExtras()
            {
                return new List<Customization>
                {
                    new Customization { Name = "Extra Wasabi", Price = 2, Icon = "/images/icons/wasabi.png" },
                    new Customization { Name = "Extra Ginger", Price = 3, Icon = "/images/icons/ginger.png" }
                };
            }

            // ========== FAST FOOD RESTAURANTS ================

            // 1. Burger King
            var bk = new Restaurant
            {
                Id = "bk",
                Name = "Burger King",
                Category = "Fast Food",
                DeliveryTime = "20-30 min",
                CoverColor = "#FF6B35",
                ImageUrl = "/images/restaurants/burgerking.jpg",
                Factory = new FastFoodFactory(),
                Rating = 4.5,
                Reviews = 2100,
                Menu = new List<MenuItem>()
            };
            bk.Menu.Add(new MenuItem { Id = 1, Name = "Whopper", Price = 55, Type = "burger", ImageUrl = "/images/menu/whopper.jpg", Description = "Flame-grilled beef patty", IsPopular = true, Customizations = GetBurgerExtras() });
            bk.Menu.Add(new MenuItem { Id = 2, Name = "Chicken Royale", Price = 50, Type = "chicken", ImageUrl = "/images/menu/chicken-royal.jpg", Description = "Crispy chicken burger", IsPopular = true, Customizations = GetChickenExtras() });
            bk.Menu.Add(new MenuItem { Id = 3, Name = "Bacon Double Cheeseburger", Price = 65, Type = "burger", ImageUrl = "/images/menu/king-double.png", Description = "Double beef with bacon", IsPopular = false, Customizations = GetBurgerExtras() });
            bk.Menu.Add(new MenuItem { Id = 4, Name = "Onion Rings", Price = 25, Type = "sides", ImageUrl = "/images/menu/king-onions.png", Description = "Crispy onion rings", IsPopular = false, Customizations = new List<Customization>() });
            _restaurants.Add(bk);

            // 2. Chick-fil-A
            var cfa = new Restaurant
            {
                Id = "cfa",
                Name = "Chick-fil-A",
                Category = "Fast Food",
                DeliveryTime = "20-30 min",
                CoverColor = "#E41C2B",
                ImageUrl = "/images/restaurants/chickfil-a.png",
                Factory = new FastFoodFactory(),
                Rating = 4.7,
                Reviews = 2500,
                Menu = new List<MenuItem>()
            };
            cfa.Menu.Add(new MenuItem { Id = 5, Name = "Original Chicken Sandwich", Price = 48, Type = "chicken", ImageUrl = "/images/menu/chicken-sandwich.jpg", Description = "Classic chicken sandwich", IsPopular = true, Customizations = GetChickenExtras() });
            cfa.Menu.Add(new MenuItem { Id = 6, Name = "Spicy Chicken Sandwich", Price = 50, Type = "chicken", ImageUrl = "/images/menu/spicyDeluxe.png", Description = "Spicy version", IsPopular = true, Customizations = GetChickenExtras() });
            cfa.Menu.Add(new MenuItem { Id = 7, Name = "Chicken Nuggets (12pc)", Price = 45, Type = "chicken", ImageUrl = "/images/menu/chick-nuggets.png", Description = "Bite-sized chicken", IsPopular = false, Customizations = GetChickenExtras() });
            cfa.Menu.Add(new MenuItem { Id = 8, Name = "Waffle Fries", Price = 28, Type = "sides", ImageUrl = "/images/menu/wafflefries.png", Description = "Waffle-cut fries", IsPopular = false, Customizations = new List<Customization>() });
            _restaurants.Add(cfa);

            // 3. ORLANDO PIRATES
            var pirates = new Restaurant
            {
                Id = "pirates",
                Name = "Orlando Pirates - Buccaneers Cafe",
                Category = "Fast Food",
                DeliveryTime = "25-35 min",
                CoverColor = "#000000",
                ImageUrl = "/images/restaurants/Orlando.png",
                Factory = new FastFoodFactory(),
                Rating = 4.9,
                Reviews = 3500,
                Menu = new List<MenuItem>()
            };
            pirates.Menu.Add(new MenuItem { Id = 9, Name = "Buccaneer Burger", Price = 85, Type = "burger", ImageUrl = "/images/menu/buccaneer.jpg", Description = "Double beef patty with special sea salt sauce", IsPopular = true, Customizations = GetBurgerExtras() });
            pirates.Menu.Add(new MenuItem { Id = 10, Name = "Sea Robber Chicken Wings", Price = 75, Type = "chicken", ImageUrl = "/images/menu/chicken-wings.jpg", Description = "12 spicy wings with pirate sauce", IsPopular = true, Customizations = GetChickenExtras() });
            pirates.Menu.Add(new MenuItem { Id = 11, Name = "Treble Winner Pizza", Price = 110, Type = "pizza", ImageUrl = "/images/menu/blackpearl.png", Description = "Pepperoni, olives, and jalapeños", IsPopular = false, Customizations = GetPizzaExtras() });
            pirates.Menu.Add(new MenuItem { Id = 12, Name = "Ghost Ship Nachos", Price = 65, Type = "sides", ImageUrl = "/images/menu/ghost-nachos.png", Description = "Loaded nachos with cheese and salsa", IsPopular = false, Customizations = new List<Customization>() });
            _restaurants.Add(pirates);

            // ========== PREMIUM RESTAURANTS ==========

            // 4. LIVERPOOL FC
            var liverpool = new Restaurant
            {
                Id = "liverpool",
                Name = "Liverpool FC - The Reds Kop Cafe",
                Category = "Premium",
                DeliveryTime = "40-55 min",
                CoverColor = "#C8102E",
                ImageUrl = "/images/restaurants/Logo-Liverpool.png",
                Factory = new PremiumFactory(),
                Rating = 4.8,
                Reviews = 3200,
                Menu = new List<MenuItem>()
            };
            liverpool.Menu.Add(new MenuItem { Id = 13, Name = "Kop Legend Chicken", Price = 85, Type = "chicken", ImageUrl = "/images/menu/koplegend.png", Description = "Grilled chicken with Anfield seasoning", IsPopular = true, Customizations = GetChickenExtras() });
            liverpool.Menu.Add(new MenuItem { Id = 14, Name = "European Night Pasta", Price = 125, Type = "pasta", ImageUrl = "/images/menu/europasta.png", Description = "Creamy pasta with Italian herbs", IsPopular = true, Customizations = GetPastaExtras() });
            liverpool.Menu.Add(new MenuItem { Id = 15, Name = "Mighty Reds Pizza", Price = 120, Type = "pizza", ImageUrl = "/images/menu/mighty-reds.png", Description = "Italian sausage, peppers, onions", IsPopular = false, Customizations = GetPizzaExtras() });
            liverpool.Menu.Add(new MenuItem { Id = 16, Name = "Anfield Nachos", Price = 70, Type = "sides", ImageUrl = "/images/menu/anfield-nachos.png", Description = "Loaded nachos with cheese and jalapeños", IsPopular = false, Customizations = new List<Customization>() });
            _restaurants.Add(liverpool);

            // 5. The Cheesecake Factory
            var cheesecake = new Restaurant
            {
                Id = "cheesecake",
                Name = "The Cheesecake Factory",
                Category = "Premium",
                DeliveryTime = "50-70 min",
                CoverColor = "#9B59B6",
                ImageUrl = "/images/restaurants/Cheesecake-Factory-Logo.png",
                Factory = new PremiumFactory(),
                Rating = 4.8,
                Reviews = 3200,
                Menu = new List<MenuItem>()
            };
            cheesecake.Menu.Add(new MenuItem { Id = 17, Name = "Chicken Parmesan", Price = 145, Type = "pasta", ImageUrl = "/images/menu/creamy-pastsa.png", Description = "Breaded chicken with pasta", IsPopular = true, Customizations = GetPastaExtras() });
            cheesecake.Menu.Add(new MenuItem { Id = 18, Name = "Grilled Salmon", Price = 165, Type = "fish", ImageUrl = "/images/menu/grilled-salmon.jpg", Description = "Fresh Atlantic salmon", IsPopular = true, Customizations = new List<Customization> { new Customization { Name = "Lemon Butter", Price = 5, Icon = "/images/icons/lemon.png" } } });
            cheesecake.Menu.Add(new MenuItem { Id = 19, Name = "Original Cheesecake", Price = 55, Type = "dessert", ImageUrl = "/images/menu/original-cheesecake.jpg", Description = "Classic cheesecake", IsPopular = false, Customizations = new List<Customization> { new Customization { Name = "Strawberry Topping", Price = 5, Icon = "/images/icons/strawberry.png" } } });
            cheesecake.Menu.Add(new MenuItem { Id = 20, Name = "Burger", Price = 95, Type = "burger", ImageUrl = "/images/menu/premium-burger.jpg", Description = "Premium burger", IsPopular = false, Customizations = GetBurgerExtras() });
            _restaurants.Add(cheesecake);

            // 6. Nobu
            var nobu = new Restaurant
            {
                Id = "nobu",
                Name = "Nobu",
                Category = "Premium",
                DeliveryTime = "45-65 min",
                CoverColor = "#E67E22",
                ImageUrl = "/images/restaurants/nobu.png",
                Factory = new PremiumFactory(),
                Rating = 4.8,
                Reviews = 2100,
                Menu = new List<MenuItem>()
            };
            nobu.Menu.Add(new MenuItem { Id = 21, Name = "Black Cod Miso", Price = 155, Type = "fish", ImageUrl = "/images/menu/Black-Cod.jpg", Description = "Butterfish marinated in miso", IsPopular = true, Customizations = new List<Customization> { new Customization { Name = "Extra Miso", Price = 5, Icon = "/images/icons/miso.png" } } });
            nobu.Menu.Add(new MenuItem { Id = 22, Name = "Yellowtail Sashimi", Price = 145, Type = "sushi", ImageUrl = "/images/menu/yellowtail-sashimi.jpg", Description = "Fresh yellowtail", IsPopular = true, Customizations = GetSushiExtras() });
            nobu.Menu.Add(new MenuItem { Id = 23, Name = "Sushi Roll Set", Price = 125, Type = "sushi", ImageUrl = "/images/menu/sushi-roll.jpg", Description = "Assorted rolls", IsPopular = false, Customizations = GetSushiExtras() });
            nobu.Menu.Add(new MenuItem { Id = 24, Name = "Rock Shrimp Tempura", Price = 135, Type = "shrimp", ImageUrl = "/images/menu/rock-shrimp.jpg", Description = "Crispy shrimp", IsPopular = false, Customizations = new List<Customization> { new Customization { Name = "Spicy Mayo", Price = 4, Icon = "/images/icons/mayo.png" } } });
            _restaurants.Add(nobu);

            // ========== HEALTHY RESTAURANTS ==========

            // 7. Sweetgreen
            var sweetgreen = new Restaurant
            {
                Id = "sweetgreen",
                Name = "Sweetgreen",
                Category = "Healthy",
                DeliveryTime = "25-35 min",
                CoverColor = "#2ECC71",
                ImageUrl = "/images/restaurants/Sweetgreen-Logo.png",
                Factory = new HealthyFactory(),
                Rating = 4.6,
                Reviews = 1950,
                Menu = new List<MenuItem>()
            };
            sweetgreen.Menu.Add(new MenuItem { Id = 25, Name = "Harvest Bowl", Price = 65, Type = "salad", ImageUrl = "/images/menu/Bowl_Harvest.png", Description = "Kale, sweet potato, chicken", IsPopular = true, Customizations = GetSaladExtras() });
            sweetgreen.Menu.Add(new MenuItem { Id = 26, Name = "Guacamole Greens", Price = 60, Type = "salad", ImageUrl = "/images/menu/guacamole-greens.jpg", Description = "Avocado, chicken, greens", IsPopular = true, Customizations = GetSaladExtras() });
            sweetgreen.Menu.Add(new MenuItem { Id = 27, Name = "Fruit Cup", Price = 25, Type = "sides", ImageUrl = "/images/menu/fruit-cup.jpg", Description = "Fresh mixed fruit", IsPopular = false, Customizations = new List<Customization>() });
            sweetgreen.Menu.Add(new MenuItem { Id = 28, Name = "Buffalo Chicken Bowl", Price = 68, Type = "bowl", ImageUrl = "/images/menu/buffulo-bowl.jpg", Description = "Spicy chicken bowl", IsPopular = false, Customizations = GetChickenExtras() });
            _restaurants.Add(sweetgreen);

            // 8. True Food Kitchen
            var truefood = new Restaurant
            {
                Id = "truefood",
                Name = "True Food Kitchen",
                Category = "Healthy",
                DeliveryTime = "30-40 min",
                CoverColor = "#27AE60",
                ImageUrl = "/images/restaurants/true-food-kitchen.jpg",
                Factory = new HealthyFactory(),
                Rating = 4.7,
                Reviews = 1700,
                Menu = new List<MenuItem>()
            };
            truefood.Menu.Add(new MenuItem { Id = 29, Name = "Ancient Grains Bowl", Price = 70, Type = "bowl", ImageUrl = "/images/menu/ancient-grains.jpg", Description = "Quinoa, sweet potato, kale", IsPopular = true, Customizations = GetSaladExtras() });
            truefood.Menu.Add(new MenuItem { Id = 30, Name = "Turkey Burger", Price = 62, Type = "burger", ImageUrl = "/images/menu/urkey_burger.jpg", Description = "Lean turkey patty", IsPopular = true, Customizations = GetBurgerExtras() });
            truefood.Menu.Add(new MenuItem { Id = 31, Name = "Kale Salad", Price = 55, Type = "salad", ImageUrl = "/images/menu/kale-salad.jpg", Description = "Massaged kale salad", IsPopular = false, Customizations = GetSaladExtras() });
            truefood.Menu.Add(new MenuItem { Id = 32, Name = "Spaghetti Squash", Price = 68, Type = "pasta", ImageUrl = "/images/menu/spaghetti-squash.jpg", Description = "Healthy pasta alternative", IsPopular = false, Customizations = GetPastaExtras() });
            _restaurants.Add(truefood);

            // 9. Dig Inn
            var dig = new Restaurant
            {
                Id = "dig",
                Name = "Dig Inn",
                Category = "Healthy",
                DeliveryTime = "25-35 min",
                CoverColor = "#1ABC9C",
                ImageUrl = "/images/restaurants/DigInn_Company_Logo.png",
                Factory = new HealthyFactory(),
                Rating = 4.5,
                Reviews = 1400,
                Menu = new List<MenuItem>()
            };
            dig.Menu.Add(new MenuItem { Id = 33, Name = "Farm Bowl", Price = 58, Type = "bowl", ImageUrl = "/images/menu/farm-bowl.jpg", Description = "Roasted vegetables, grains", IsPopular = true, Customizations = GetSaladExtras() });
            dig.Menu.Add(new MenuItem { Id = 34, Name = "Harvest Chicken", Price = 64, Type = "chicken", ImageUrl = "/images/menu/harvest-chicken.jpg", Description = "Grilled chicken breast", IsPopular = true, Customizations = GetChickenExtras() });
            dig.Menu.Add(new MenuItem { Id = 35, Name = "Market Salad", Price = 52, Type = "salad", ImageUrl = "/images/menu/market-salad.jpg", Description = "Seasonal market salad", IsPopular = false, Customizations = GetSaladExtras() });
            dig.Menu.Add(new MenuItem { Id = 36, Name = "Sweet Potatoes", Price = 22, Type = "sides", ImageUrl = "/images/menu/sweet-potatoes.jpg", Description = "Roasted sweet potatoes", IsPopular = false, Customizations = new List<Customization>() });
            _restaurants.Add(dig);
        }

        public IActionResult Index()
        {
            ViewBag.CartCount = _cart.Count;
            ViewBag.LoyaltyPoints = _loyaltyPoints;
            return View(_restaurants);
        }

        [HttpGet]
        public IActionResult GetCart()
        {
            var subtotal = _cart.Sum(c => c.TotalPrice);
            var deliveryFee = _cart.Count > 0 ? 25m : 0;
            var total = subtotal + deliveryFee;

            return Json(new
            {
                Items = _cart.Select((c, i) => new
                {
                    Index = i,
                    c.RestaurantName,
                    ItemName = c.Item.Name,
                    ItemIcon = c.Item.ImageUrl ?? c.Item.Icon,
                    TotalPrice = c.TotalPrice,
                    Extras = c.Extras ?? new List<string>()
                }),
                Subtotal = subtotal,
                DeliveryFee = deliveryFee,
                Total = total,
                ItemCount = _cart.Count
            });
        }

        [HttpGet]
        public IActionResult GetLoyaltyPoints()
        {
            return Json(new { points = _loyaltyPoints });
        }

        [HttpPost]
        public IActionResult ApplyPromo(string promoCode)
        {
            var subtotal = _cart.Sum(c => c.TotalPrice);
            decimal discount = 0;
            bool success = false;
            string message = "";

            if (_usedPromoCodes.Contains(promoCode))
            {
                message = "Promo code already used!";
            }
            else if (promoCode == "CRAVE20")
            {
                discount = subtotal * 0.2m;
                success = true;
                message = "20% discount applied!";
                _usedPromoCodes.Add(promoCode);
            }
            else if (promoCode == "CRAVE10")
            {
                discount = subtotal * 0.1m;
                success = true;
                message = "10% discount applied!";
                _usedPromoCodes.Add(promoCode);
            }
            else if (promoCode == "FREEDELIVERY")
            {
                discount = 25;
                success = true;
                message = "Free delivery applied!";
                _usedPromoCodes.Add(promoCode);
            }
            else
            {
                message = "Invalid promo code";
            }

            return Json(new { success = success, discount = discount, message = message });
        }

        [HttpPost]
        public IActionResult ApplyPoints(int points)
        {
            if (points > _loyaltyPoints) points = _loyaltyPoints;
            decimal discount = points / 10m;
            var subtotal = _cart.Sum(c => c.TotalPrice);
            if (discount > subtotal) discount = subtotal;

            if (points > 0) _loyaltyPoints -= points;

            return Json(new { success = true, discount = discount, remainingPoints = _loyaltyPoints });
        }

        [HttpGet]
        public IActionResult GetLoyaltyHistory()
        {
            var history = new
            {
                CurrentPoints = _loyaltyPoints,
                PointsEarned = _pointsEarnedHistory,
                PointsRedeemed = _pointsRedeemedHistory,
                NextReward = 1000 - (_loyaltyPoints % 1000),
                Tier = _loyaltyPoints >= 1000 ? "Gold" : _loyaltyPoints >= 500 ? "Silver" : "Bronze"
            };
            return Json(history);
        }

        [HttpPost]
        public IActionResult AddToCart(string restaurantId, int itemId, List<string> extras)
        {
            var restaurant = _restaurants.Find(r => r.Id == restaurantId);
            if (restaurant == null) return Json(new { success = false, message = "Restaurant not found" });

            var menuItem = restaurant.Menu.Find(m => m.Id == itemId);
            if (menuItem == null) return Json(new { success = false, message = "Item not found" });

            decimal finalPrice = menuItem.Price;
            if (extras != null && extras.Any())
            {
                foreach (var extra in extras)
                {
                    var cust = menuItem.Customizations.Find(c => c.Name == extra);
                    if (cust != null) finalPrice += cust.Price;
                }
            }

            var cartItem = new CartItem
            {
                Id = Guid.NewGuid().ToString(),
                RestaurantId = restaurantId,
                RestaurantName = restaurant.Name,
                Item = menuItem,
                Extras = extras ?? new List<string>(),
                TotalPrice = finalPrice,
                ItemName = menuItem.Name,
                ItemIcon = menuItem.ImageUrl ?? menuItem.Icon
            };

            _cart.Add(cartItem);

            OrderLogger.Instance.Log($"{menuItem.Name} added to cart from {restaurant.Name} - R{finalPrice:F2}");
            if (extras != null && extras.Any())
            {
                OrderLogger.Instance.Log($"  + {string.Join(", ", extras)}");
            }

            return Json(new { success = true, cartCount = _cart.Count });
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int index)
        {
            if (index >= 0 && index < _cart.Count) _cart.RemoveAt(index);
            return Json(new { success = true, cartCount = _cart.Count });
        }

        [HttpPost]
        public IActionResult ClearCart()
        {
            _cart.Clear();
            return Json(new { success = true });
        }

        // SAVE/LOAD BONUS
        [HttpPost]
        public IActionResult SaveCart()
        {
            _saveLoadService.SaveCart(_cart, _loyaltyPoints);
            return Json(new { success = true, message = "Cart saved successfully!" });
        }

        [HttpPost]
        public IActionResult LoadCart()
        {
            var saveData = _saveLoadService.LoadCart();
            if (saveData != null && saveData.Items != null)
            {
                _cart = saveData.Items;
                _loyaltyPoints = saveData.LoyaltyPoints;
                return Json(new { success = true, message = $"Loaded {_cart.Count} items from save!", cartCount = _cart.Count });
            }
            return Json(new { success = false, message = "No saved cart found!" });
        }

        [HttpGet]
        public IActionResult GetSaveFiles()
        {
            var files = _saveLoadService.GetSaveFiles();
            return Json(files);
        }

        // DRIVER TRACKING BONUS
        [HttpGet]
        public IActionResult GetDriverStatus()
        {
            var driverInfo = _driverService.GetCurrentStatus();
            return Json(new
            {
                success = true,
                driverName = driverInfo.DriverName,
                status = driverInfo.Status,
                eta = driverInfo.EtaMinutes,
                latitude = driverInfo.Latitude,
                longitude = driverInfo.Longitude,
                history = driverInfo.History
            });
        }

        [HttpPost]
        public IActionResult AssignDriver()
        {
            if (_cart.Count > 0)
            {
                var restaurant = _cart.First().RestaurantName;
                _driverService.AssignDriver(restaurant, "Customer Address");
                return Json(new { success = true, message = "Driver assigned!" });
            }
            return Json(new { success = false, message = "Cart is empty!" });
        }

        [HttpGet]
        public IActionResult UpdateDriverTracking()
        {
            var driverInfo = _driverService.UpdateTracking();
            return Json(new
            {
                success = true,
                driverName = driverInfo.DriverName,
                status = driverInfo.Status,
                eta = driverInfo.EtaMinutes,
                latitude = driverInfo.Latitude,
                longitude = driverInfo.Longitude,
                history = driverInfo.History
            });
        }

        // LOYALTY ENHANCEMENTS
        [HttpGet]
        public IActionResult GetLoyaltyDetails()
        {
            string tier = _loyaltyPoints >= 1000 ? "Gold" : _loyaltyPoints >= 500 ? "Silver" : "Bronze";
            string nextReward = _loyaltyPoints >= 1000 ? "2000" : _loyaltyPoints >= 500 ? "1000" : "500";

            return Json(new
            {
                currentPoints = _loyaltyPoints,
                tier = tier,
                pointsToNextTier = int.Parse(nextReward) - _loyaltyPoints,
                benefits = tier switch
                {
                    "Gold" => "15% off all orders, Free delivery, Priority support",
                    "Silver" => "10% off all orders, Free delivery over R100",
                    "Bronze" => "5% off orders over R150",
                    _ => "Standard benefits"
                }
            });
        }

        [HttpPost]
        public IActionResult RedeemReward(string rewardType)
        {
            int pointsNeeded = rewardType switch
            {
                "Free Delivery" => 100,
                "5% Off" => 150,
                "10% Off" => 250,
                "Free Side" => 80,
                _ => 0
            };

            if (_loyaltyPoints >= pointsNeeded && pointsNeeded > 0)
            {
                _loyaltyPoints -= pointsNeeded;
                _pointsRedeemedHistory.Add(pointsNeeded);
                return Json(new { success = true, message = $"{rewardType} redeemed!", remainingPoints = _loyaltyPoints });
            }

            return Json(new { success = false, message = $"Need {pointsNeeded} points!" });
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(string paymentMethod, decimal total)
        {
            if (_cart.Count == 0) return Json(new { success = false, message = "Cart is empty" });

            Console.WriteLine("\n╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              🔔 ORDER NOTIFICATIONS 🔔                     ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

            var logger = OrderLogger.Instance;
            var items = string.Join(", ", _cart.Select(c => c.ItemName));
            var restaurant = _cart.First().RestaurantName;
            logger.RecordOrder(total, restaurant, items);

            var payment = PaymentFactory.Create(paymentMethod);
            var paymentResult = payment.Pay(total);
            logger.Log(paymentResult);

            var notifier = new OrderNotifier();
            notifier.Attach(new CustomerObserver());
            notifier.Attach(new RestaurantObserver());
            notifier.Attach(new DriverObserver());

            notifier.Notify($"📦 Order #{logger.TotalOrders} received by {restaurant}");
            await Task.Delay(800);

            notifier.Notify($"👨‍🍳 {restaurant} is preparing: {items}");
            await Task.Delay(1200);

            notifier.Notify($"🚗 Driver assigned to pick up your order");
            await Task.Delay(1000);

            notifier.Notify($"✅ Meal ready! Driver is picking up");
            await Task.Delay(800);

            notifier.Notify($"🚚 Order out for delivery! ETA: 15-20 min");
            await Task.Delay(1000);

            notifier.Notify($"📍 Driver is 5 minutes away");
            await Task.Delay(800);

            notifier.Notify($"🎉 ORDER DELIVERED! Enjoy your meal!");

            Console.WriteLine("\n╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║  ✅ Order Complete!                                      ║");
            Console.WriteLine($"║  📊 {logger.GetSummary()}                               ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

            var pointsEarned = (int)(total * 0.1m);
            _loyaltyPoints += pointsEarned;
            _pointsEarnedHistory.Add(pointsEarned);

            _cart.Clear();

            return Json(new
            {
                success = true,
                message = "Order placed successfully!",
                paymentMessage = paymentResult,
                notifications = new List<string>(),
                loggerSummary = logger.GetSummary(),
                orderId = logger.TotalOrders,
                pointsEarned = pointsEarned,
                totalPoints = _loyaltyPoints
            });
        }
    }
}