using Microsoft.AspNetCore.Mvc;
using CraveCart.Models;
using CraveCart.Models.DesignPatterns;
using CraveCart.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.IO;

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

            if (_restaurantOrders.Count == 0 && _restaurantOwners.Any())
            {
                var random = new Random();
                foreach (var owner in _restaurantOwners)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var statuses = new[] { "New", "Preparing", "Ready", "OutForDelivery", "Completed" };
                        var status = statuses[random.Next(0, 5)];
                        var orderTotal = 50 + random.Next(0, 150);

                        _restaurantOrders.Add(new RestaurantOrder
                        {
                            OrderId = 2000 + i + owner.Id * 10,
                            OrderTime = DateTime.Now.AddDays(-random.Next(0, 7)),
                            RestaurantId = owner.RestaurantId,
                            RestaurantName = owner.RestaurantName,
                            CustomerName = $"Customer {i + 1}",
                            CustomerAddress = $"{123 + i} Main Street",
                            CustomerPhone = $"081{random.Next(1000000, 9999999)}",
                            Items = new List<OrderItemDetail>
                            {
                                new OrderItemDetail { Name = "Sample Dish", Quantity = 1, Price = orderTotal - 25, Customizations = new List<string>() }
                            },
                            Subtotal = orderTotal - 25,
                            DeliveryFee = 25,
                            Total = orderTotal,
                            Status = status
                        });
                    }
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
                OrderLogger.Instance.Log($"Restaurant '{owner.RestaurantName}' logged in");
                return RedirectToAction("Dashboard");
            }
            ViewBag.Error = "Invalid credentials";
            return View();
        }

        [HttpPost]
        public IActionResult Logout()
        {
            if (_currentRestaurant != null)
            {
                OrderLogger.Instance.Log($"Restaurant '{_currentRestaurant.RestaurantName}' logged out");
                _currentRestaurant = null;
            }
            return RedirectToAction("Index", "Home");
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

                // SYNC TO CUSTOMER - Update HomeController's order status
                HomeController.UpdateOrderStatus(orderId, status);

                var statusMessage = status switch
                {
                    "Preparing" => $"👨‍🍳 Your order #{orderId} is now being prepared!",
                    "Ready" => $"✅ Your order #{orderId} is ready for pickup!",
                    "OutForDelivery" => $"🚚 Your order #{orderId} is out for delivery!",
                    "Completed" => $"🎉 Your order #{orderId} has been delivered!",
                    _ => $"📋 Order #{orderId} status: {status}"
                };

                var notifier = new OrderNotifier();
                notifier.Attach(new CustomerObserver());
                notifier.Notify(statusMessage);

                if (status == "Ready")
                {
                    order.EstimatedReadyTime = DateTime.Now.AddMinutes(5);
                    DriverController.NotifyNewReadyOrder(orderId, order.RestaurantName);
                }

                OrderLogger.Instance.Log($"Order #{orderId} status changed to {status} by restaurant '{order.RestaurantName}'");
            }
            return Json(new { success = true, status = status });
        }

        [HttpGet]
        public IActionResult OrderDetails(int id)
        {
            if (_currentRestaurant == null) return RedirectToAction("Login");
            var order = _restaurantOrders.FirstOrDefault(o => o.OrderId == id);
            if (order == null) return NotFound();

            if (string.IsNullOrEmpty(order.RestaurantName))
                order.RestaurantName = _currentRestaurant.RestaurantName;

            return View(order);
        }

        // ========== REAL-TIME ORDER STATUS FOR CUSTOMER POLLING ==========
        [HttpGet]
        public IActionResult GetOrderStatus(int orderId)
        {
            var order = _restaurantOrders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
                return Json(new { status = "Not Found" });

            var driverInfo = DriverController.GetDriverForOrder(orderId);

            return Json(new
            {
                orderId = order.OrderId,
                status = order.Status,
                restaurantName = order.RestaurantName,
                estimatedReadyTime = order.EstimatedReadyTime?.ToString("hh:mm tt"),
                driverName = driverInfo?.Name,
                driverStatus = driverInfo?.CurrentStatus,
                eta = driverInfo?.EstimatedMinutes
            });
        }

        // ========== MENU MANAGEMENT ==========

        [HttpGet]
        public IActionResult ManageMenu()
        {
            if (_currentRestaurant == null) return RedirectToAction("Login");

            var restaurant = HomeController.GetAllRestaurants()?.FirstOrDefault(r => r.Id == _currentRestaurant.RestaurantId);
            if (restaurant == null) return NotFound();

            ViewBag.RestaurantName = _currentRestaurant.RestaurantName;
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
                OrderLogger.Instance.Log($"Menu item '{item.Name}' (R{item.Price}) added to {restaurant.Name}");
            }
            return RedirectToAction("ManageMenu");
        }

        [HttpPost]
        public IActionResult UpdateMenuItem(MenuItem item)
        {
            var restaurant = HomeController.GetAllRestaurants()?.FirstOrDefault(r => r.Id == _currentRestaurant?.RestaurantId);
            var existingItem = restaurant?.Menu.FirstOrDefault(m => m.Id == item.Id);
            if (existingItem != null)
            {
                existingItem.Name = item.Name;
                existingItem.Price = item.Price;
                existingItem.Description = item.Description;
                existingItem.IsPopular = item.IsPopular;
                OrderLogger.Instance.Log($"Menu item '{item.Name}' updated in {restaurant.Name}");
            }
            return RedirectToAction("ManageMenu");
        }

        [HttpPost]
        public IActionResult DeleteMenuItem(int id)
        {
            var restaurant = HomeController.GetAllRestaurants()?.FirstOrDefault(r => r.Id == _currentRestaurant?.RestaurantId);
            var item = restaurant?.Menu.FirstOrDefault(m => m.Id == id);
            if (item != null)
            {
                restaurant.Menu.Remove(item);
                OrderLogger.Instance.Log($"Menu item '{item.Name}' deleted from {restaurant.Name}");
            }
            return RedirectToAction("ManageMenu");
        }

        [HttpPost]
        public IActionResult ToggleOrderAcceptance(bool accept)
        {
            if (_currentRestaurant != null)
            {
                var restaurant = HomeController.GetAllRestaurants()?.FirstOrDefault(r => r.Id == _currentRestaurant.RestaurantId);
                if (restaurant != null)
                {
                    restaurant.IsAcceptingOrders = accept;
                    OrderLogger.Instance.Log($"Restaurant '{restaurant.Name}' is {(accept ? "now accepting" : "paused")} orders");

                    var notifier = new OrderNotifier();
                    notifier.Notify($"🍽️ Restaurant '{restaurant.Name}' is {(accept ? "now accepting orders" : "temporarily closed")}");
                }
            }
            return Json(new { success = true, accepting = accept });
        }

        // ========== REPORTS ==========

        [HttpGet]
        public IActionResult Reports()
        {
            if (_currentRestaurant == null) return RedirectToAction("Login");
            ViewBag.RestaurantName = _currentRestaurant.RestaurantName;
            return View();
        }

        [HttpGet]
        public IActionResult GetRestaurantStats(DateTime startDate, DateTime endDate)
        {
            if (_currentRestaurant == null) return Json(new { success = false, message = "Not authenticated" });

            var orders = _restaurantOrders
                .Where(o => o.RestaurantId == _currentRestaurant.RestaurantId && o.OrderTime >= startDate && o.OrderTime <= endDate)
                .ToList();

            var dailyStats = new List<object>();
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var dayOrders = orders.Where(o => o.OrderTime.Date == date.Date).ToList();
                dailyStats.Add(new
                {
                    date = date.ToString("MM/dd"),
                    revenue = dayOrders.Sum(o => o.Total),
                    orders = dayOrders.Count
                });
            }

            var topItems = new List<object>();
            var allItems = orders.SelectMany(o => o.Items ?? new List<OrderItemDetail>())
                .GroupBy(i => i.Name)
                .Select(g => new
                {
                    name = g.Key,
                    quantity = g.Sum(i => i.Quantity),
                    revenue = g.Sum(i => i.Price * i.Quantity)
                })
                .OrderByDescending(g => g.quantity)
                .Take(5)
                .ToList();

            var stats = new
            {
                totalOrders = orders.Count,
                totalRevenue = orders.Sum(o => o.Total),
                averageRating = 4.5,
                averagePrepTime = 18.5,
                dailyRevenue = dailyStats.Select(d => ((dynamic)d).revenue).ToList(),
                dailyOrders = dailyStats.Select(d => ((dynamic)d).orders).ToList(),
                dailyLabels = dailyStats.Select(d => ((dynamic)d).date).ToList()
            };

            var recentOrders = orders.OrderByDescending(o => o.OrderTime).Take(10).Select(o => new
            {
                orderId = o.OrderId,
                date = o.OrderTime,
                customerName = o.CustomerName,
                itemCount = o.Items?.Count ?? 0,
                total = o.Total,
                status = o.Status
            }).ToList();

            return Json(new
            {
                success = true,
                stats = stats,
                topItems = topItems,
                recentOrders = recentOrders
            });
        }

        [HttpPost]
        public IActionResult ExportReport(DateTime startDate, DateTime endDate)
        {
            if (_currentRestaurant == null) return Json(new { success = false, message = "Not authenticated" });

            try
            {
                var orders = _restaurantOrders
                    .Where(o => o.RestaurantId == _currentRestaurant.RestaurantId && o.OrderTime >= startDate && o.OrderTime <= endDate)
                    .ToList();

                var report = new
                {
                    generatedAt = DateTime.Now,
                    restaurantName = _currentRestaurant.RestaurantName,
                    startDate = startDate,
                    endDate = endDate,
                    totalOrders = orders.Count,
                    totalRevenue = orders.Sum(o => o.Total),
                    orders = orders.Select(o => new
                    {
                        o.OrderId,
                        o.OrderTime,
                        o.CustomerName,
                        o.Total,
                        o.Status
                    })
                };

                var filename = $"restaurant_report_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                var directory = Path.Combine(Directory.GetCurrentDirectory(), "Data", "RestaurantReports");

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var filePath = Path.Combine(directory, filename);
                var json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(filePath, json);

                OrderLogger.Instance.Log($"Report exported by {_currentRestaurant.RestaurantName}: {filename}");
                return Json(new { success = true, filename = filename });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult DownloadReport(string filename)
        {
            var directory = Path.Combine(Directory.GetCurrentDirectory(), "Data", "RestaurantReports");
            var filePath = Path.Combine(directory, filename);

            if (System.IO.File.Exists(filePath))
            {
                var bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "application/json", filename);
            }
            return NotFound();
        }

        // ========== REAL-TIME NOTIFICATIONS ==========
        [HttpGet]
        public IActionResult GetNewOrders()
        {
            if (_currentRestaurant == null) return Json(new List<object>());

            var newOrders = _restaurantOrders
                .Where(o => o.RestaurantId == _currentRestaurant.RestaurantId && o.Status == "New")
                .Select(o => new { o.OrderId, o.OrderTime, o.Total, o.CustomerName })
                .ToList();

            return Json(newOrders);
        }

        // ========== STATIC METHODS FOR CROSS-SYSTEM INTEGRATION ==========
        public static void AddOrder(RestaurantOrder order)
        {
            _restaurantOrders.Add(order);
            Console.WriteLine($"🍽️ Restaurant System: Order #{order.OrderId} added to {order.RestaurantName}");

            // Sync status to customer
            HomeController.UpdateOrderStatus(order.OrderId, "New");

            var notifier = new OrderNotifier();
            notifier.Attach(new RestaurantObserver());
            notifier.Notify($"📦 NEW ORDER #{order.OrderId} received at {order.RestaurantName}");

            OrderLogger.Instance.Log($"Order #{order.OrderId} received by restaurant '{order.RestaurantName}'");
        }

        public static void UpdateOrderStatusStatic(int orderId, string status)
        {
            var order = _restaurantOrders.FirstOrDefault(o => o.OrderId == orderId);
            if (order != null)
            {
                order.Status = status;
                Console.WriteLine($"🍽️ Restaurant System: Order #{orderId} status -> {status}");

                // Sync to customer
                HomeController.UpdateOrderStatus(orderId, status);

                var notifier = new OrderNotifier();
                notifier.Attach(new CustomerObserver());
                notifier.Notify($"📋 Order #{orderId} status: {status}");
            }
        }

        public static List<RestaurantOrder> GetRestaurantOrders(string? restaurantId = null)
        {
            if (string.IsNullOrEmpty(restaurantId))
                return _restaurantOrders;
            return _restaurantOrders.Where(o => o.RestaurantId == restaurantId).ToList();
        }

        public static void RegisterRestaurant(Restaurant restaurant)
        {
            if (!_restaurantOwners.Any(o => o.RestaurantId == restaurant.Id))
            {
                _restaurantOwners.Add(new RestaurantOwner
                {
                    Id = GenerateNextOwnerIdStatic(),
                    Email = restaurant.OwnerEmail ?? $"{restaurant.Name.ToLower().Replace(" ", "")}@restaurant.com",
                    Password = "rest123",
                    RestaurantId = restaurant.Id,
                    RestaurantName = restaurant.Name,
                    IsVerified = true
                });
                OrderLogger.Instance.Log($"New restaurant registered: '{restaurant.Name}'");
            }
        }

        private static int GenerateNextOwnerIdStatic()
        {
            return _restaurantOwners.Any() ? _restaurantOwners.Max(o => o.Id) + 1 : 1;
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
        public string? RestaurantName { get; set; }
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