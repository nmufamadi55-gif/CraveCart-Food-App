using Microsoft.AspNetCore.Mvc;
using CraveCart.Models;
using CraveCart.Models.DesignPatterns;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraveCart.Controllers
{
    public class RestaurantController : Controller
    {
        private static RestaurantOwner? _currentRestaurant;
        private static List<RestaurantOwner> _restaurantOwners = new();
        private static List<RestaurantOrder> _restaurantOrders = new();

        public RestaurantController()
        {
            InitializeRestaurantData();
        }

        private void InitializeRestaurantData()
        {
            var restaurants = HomeController.GetAllRestaurants();
            if (restaurants != null && _restaurantOwners.Count == 0)
            {
                foreach (var restaurant in restaurants)
                {
                    _restaurantOwners.Add(new RestaurantOwner
                    {
                        Id = GenerateNextOwnerId(),
                        Email = $"{restaurant.Name.ToLower().Replace(" ", "")}@restaurant.com",
                        Password = "rest123",
                        RestaurantId = restaurant.Id,
                        RestaurantName = restaurant.Name,
                        IsVerified = true
                    });
                }
            }
        }

        // ========== AUTHENTICATION ==========

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var owner = _restaurantOwners.FirstOrDefault(o => o.Email == email && o.Password == password);
            if (owner != null)
            {
                _currentRestaurant = owner;
                return RedirectToAction("Dashboard");
            }
            ViewBag.Error = "Invalid credentials";
            return View();
        }

        [HttpPost]
        public IActionResult Logout()
        {
            _currentRestaurant = null;
            return RedirectToAction("Login");
        }

        // ========== DASHBOARD ==========

        [HttpGet]
        public IActionResult Dashboard()
        {
            if (_currentRestaurant == null) return RedirectToAction("Login");

            ViewBag.RestaurantName = _currentRestaurant.RestaurantName;
            ViewBag.Stats = GetRestaurantStats();
            ViewBag.RecentOrders = GetRecentOrders(10);
            return View();
        }

        private RestaurantStats GetRestaurantStats()
        {
            var orders = _restaurantOrders.Where(o => o.RestaurantId == _currentRestaurant?.RestaurantId).ToList();
            var todayOrders = orders.Where(o => o.OrderTime.Date == DateTime.Today).ToList();

            return new RestaurantStats
            {
                TodayOrders = todayOrders.Count,
                TodayRevenue = todayOrders.Sum(o => o.Total),
                WeeklyOrders = orders.Count(o => o.OrderTime > DateTime.Now.AddDays(-7)),
                WeeklyRevenue = orders.Where(o => o.OrderTime > DateTime.Now.AddDays(-7)).Sum(o => o.Total),
                AverageRating = 4.5,
                TotalReviews = 1250,
                AveragePreparationTime = 18.5
            };
        }

        private List<RestaurantOrder> GetRecentOrders(int count)
        {
            return _restaurantOrders
                .Where(o => o.RestaurantId == _currentRestaurant?.RestaurantId)
                .OrderByDescending(o => o.OrderTime)
                .Take(count)
                .ToList();
        }

        // ========== ORDER MANAGEMENT ==========

        [HttpGet]
        public IActionResult Orders(string status = "all")
        {
            if (_currentRestaurant == null) return RedirectToAction("Login");

            var orders = _restaurantOrders.Where(o => o.RestaurantId == _currentRestaurant.RestaurantId);
            if (status != "all")
                orders = orders.Where(o => o.Status == status);

            ViewBag.CurrentStatus = status;
            return View(orders.ToList());
        }

        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, string status)
        {
            var order = _restaurantOrders.FirstOrDefault(o => o.OrderId == orderId);
            if (order != null)
            {
                order.Status = status;
                if (status == "Ready")
                    order.EstimatedReadyTime = DateTime.Now.AddMinutes(5);

                var notifier = new OrderNotifier();
                notifier.Attach(new CustomerObserver());
                notifier.Notify($"Order #{orderId} is {status}!");
            }
            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult OrderDetails(int id)
        {
            if (_currentRestaurant == null) return RedirectToAction("Login");
            var order = _restaurantOrders.FirstOrDefault(o => o.OrderId == id);
            return View(order);
        }

        // ========== MENU MANAGEMENT ==========

        [HttpGet]
        public IActionResult ManageMenu()
        {
            if (_currentRestaurant == null) return RedirectToAction("Login");

            var restaurant = HomeController.GetAllRestaurants()?.FirstOrDefault(r => r.Id == _currentRestaurant.RestaurantId);
            return View(restaurant);
        }

        [HttpPost]
        public IActionResult AddMenuItem(MenuItem item)
        {
            var restaurant = HomeController.GetAllRestaurants()?.FirstOrDefault(r => r.Id == _currentRestaurant?.RestaurantId);
            if (restaurant != null)
            {
                item.Id = GenerateNextMenuItemId();
                restaurant.Menu.Add(item);
                OrderLogger.Instance.Log($"Menu item '{item.Name}' added to {restaurant.Name}");
            }
            return RedirectToAction("ManageMenu");
        }

        [HttpPost]
        public IActionResult DeleteMenuItem(int id)
        {
            var restaurant = HomeController.GetAllRestaurants()?.FirstOrDefault(r => r.Id == _currentRestaurant?.RestaurantId);
            var item = restaurant?.Menu.FirstOrDefault(m => m.Id == id);
            if (item != null)
                restaurant.Menu.Remove(item);
            return RedirectToAction("ManageMenu");
        }

        // ========== HELPER METHODS ==========

        private int GenerateNextOwnerId()
        {
            return _restaurantOwners.Any() ? _restaurantOwners.Max(o => o.Id) + 1 : 1;
        }

        private int GenerateNextMenuItemId()
        {
            var restaurants = HomeController.GetAllRestaurants();
            var allItems = restaurants?.SelectMany(r => r.Menu) ?? new List<MenuItem>();
            return allItems.Any() ? allItems.Max(i => i.Id) + 1 : 37;
        }
    }

    // ========== MODEL CLASSES ==========

    public class RestaurantOwner
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? RestaurantId { get; set; }
        public string? RestaurantName { get; set; }
        public bool IsVerified { get; set; }
    }

    public class RestaurantOrder
    {
        public int OrderId { get; set; }
        public DateTime OrderTime { get; set; }
        public string? RestaurantId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerPhone { get; set; }
        public List<OrderItemDetail>? Items { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Total { get; set; }
        public string? Status { get; set; }
        public DateTime? EstimatedReadyTime { get; set; }
    }

    public class OrderItemDetail
    {
        public string? Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public List<string>? Customizations { get; set; }
    }

    public class RestaurantStats
    {
        public int TodayOrders { get; set; }
        public decimal TodayRevenue { get; set; }
        public int WeeklyOrders { get; set; }
        public decimal WeeklyRevenue { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public double AveragePreparationTime { get; set; }
    }
}