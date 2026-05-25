using Microsoft.AspNetCore.Mvc;
using CraveCart.Models.Admin;
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
    public class AdminController : Controller
    {
        private static AdminUser? _currentAdmin;
        private static List<RestaurantApproval> _restaurantApplications = new();
        private static List<DriverApproval> _driverApplications = new();
        private static SystemSettings _settings = new();
        private static List<AdminUser> _admins = new();
        private static List<DriverInfo> _activeDrivers = new();

        public AdminController()
        {
            InitializeAdminData();
        }

        private void InitializeAdminData()
        {
            if (_admins.Count == 0)
            {
                _admins.Add(new AdminUser
                {
                    Id = 1,
                    Username = "admin",
                    Password = "admin123",
                    Email = "admin@cravecart.com",
                    Role = "SuperAdmin",
                    CreatedAt = DateTime.Now
                });
            }

            if (_restaurantApplications.Count == 0)
            {
                _restaurantApplications.Add(new RestaurantApproval
                {
                    Id = 1,
                    RestaurantName = "Pizza Heaven",
                    OwnerName = "John Doe",
                    OwnerEmail = "john@pizzaheaven.com",
                    Category = "Fast Food",
                    Status = "Pending",
                    SubmittedDate = DateTime.Now.AddDays(-2),
                    MenuItems = new List<MenuItemRequest>
                    {
                        new MenuItemRequest { Name = "Margherita Pizza", Price = 85, Type = "pizza", Description = "Classic cheese and tomato" },
                        new MenuItemRequest { Name = "Pepperoni Pizza", Price = 95, Type = "pizza", Description = "Spicy pepperoni" }
                    }
                });
            }

            if (_driverApplications.Count == 0)
            {
                _driverApplications.Add(new DriverApproval
                {
                    Id = 1,
                    DriverName = "Jane Smith",
                    Email = "jane@delivery.com",
                    Phone = "0821234567",
                    LicenseNumber = "DL123456",
                    VehicleType = "Scooter",
                    VehiclePlate = "ABC123",
                    Status = "Pending",
                    SubmittedDate = DateTime.Now.AddDays(-3),
                    BackgroundCheckScore = 95.5
                });
            }

            if (_activeDrivers.Count == 0)
            {
                _activeDrivers.Add(new DriverInfo
                {
                    DriverName = "Michael",
                    Email = "michael@delivery.com",
                    Phone = "0812345678",
                    Status = "Available",
                    VehicleType = "Motorcycle",
                    Rating = 4.8,
                    TotalDeliveries = 150
                });
            }
        }

        // ========== LOGIN & DASHBOARD ==========

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var admin = _admins.FirstOrDefault(a => a.Username == username && a.Password == password);
            if (admin != null)
            {
                _currentAdmin = admin;
                _currentAdmin.LastLogin = DateTime.Now;
                return RedirectToAction("Dashboard");
            }
            ViewBag.Error = "Invalid credentials";
            return View();
        }

        [HttpPost]
        public IActionResult JsonLogin([FromBody] LoginRequest request)
        {
            var admin = _admins.FirstOrDefault(a => a.Username == request.Username && a.Password == request.Password);
            if (admin != null)
            {
                _currentAdmin = admin;
                _currentAdmin.LastLogin = DateTime.Now;
                return Json(new { success = true, message = "Login successful", redirectUrl = "/Admin/Dashboard" });
            }
            return Json(new { success = false, message = "Invalid username or password" });
        }

        [HttpGet]
        public IActionResult CheckAuth() => Json(new { isLoggedIn = _currentAdmin != null });

        [HttpPost]
        public IActionResult Logout()
        {
            _currentAdmin = null;
            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            if (_currentAdmin == null) return RedirectToAction("Login");
            var stats = GetDashboardStats();
            return View(stats);
        }

        private CraveCart.Models.Admin.DashboardStats GetDashboardStats()
        {
            var logger = OrderLogger.Instance;
            var restaurants = HomeController.GetAllRestaurants();

            // Get data and convert to correct types
            var weeklyRevenueData = HomeController.GetWeeklyRevenueData();
            var categoryData = HomeController.GetOrdersByCategoryData();

            // Convert HomeController's DailyRevenue to Admin.DailyRevenue
            var weeklyRevenue = weeklyRevenueData.Select(w => new CraveCart.Models.Admin.DailyRevenue
            {
                Date = w.Date,
                Amount = w.Amount,
                OrderCount = w.OrderCount
            }).ToList();

            // Convert HomeController's OrderCount to Admin.OrderCount
            var ordersByCategory = categoryData.Select(c => new CraveCart.Models.Admin.OrderCount
            {
                Category = c.Category,
                Count = c.Count
            }).ToList();

            var topRestaurants = GetTopRestaurantsFromOrders();

            return new CraveCart.Models.Admin.DashboardStats
            {
                TotalOrders = logger.TotalOrders,
                TotalRevenue = logger.TotalRevenue,
                TotalRestaurants = restaurants?.Count ?? 0,
                TotalDrivers = _activeDrivers.Count,
                TodayRevenue = HomeController.GetTodayRevenue(),
                TodayOrders = HomeController.GetTodayOrderCount(),
                ActiveOrders = HomeController.GetActiveOrders()?.Count ?? 0,
                PendingApprovals = _restaurantApplications.Count(r => r.Status == "Pending") + _driverApplications.Count(d => d.Status == "Pending"),
                WeeklyRevenue = weeklyRevenue,
                OrdersByCategory = ordersByCategory,
                TopRestaurants = topRestaurants
            };
        }

        private List<CraveCart.Models.Admin.TopRestaurant> GetTopRestaurantsFromOrders()
        {
            var orders = HomeController.GetAllOrders();
            var topRestaurants = orders.GroupBy(o => o.RestaurantName)
                .Select(g => new CraveCart.Models.Admin.TopRestaurant
                {
                    Name = g.Key ?? "Unknown",
                    Orders = g.Count(),
                    Revenue = g.Sum(o => o.TotalAmount),
                    Rating = 4.5
                })
                .OrderByDescending(r => r.Revenue)
                .Take(5)
                .ToList();

            if (!topRestaurants.Any())
            {
                topRestaurants.Add(new CraveCart.Models.Admin.TopRestaurant { Name = "Burger King", Orders = 125, Revenue = 6875, Rating = 4.5 });
                topRestaurants.Add(new CraveCart.Models.Admin.TopRestaurant { Name = "Sweetgreen", Orders = 98, Revenue = 6370, Rating = 4.6 });
                topRestaurants.Add(new CraveCart.Models.Admin.TopRestaurant { Name = "Nobu", Orders = 76, Revenue = 11780, Rating = 4.8 });
            }
            return topRestaurants;
        }

        // ========== RESTAURANT MANAGEMENT ==========

        [HttpGet]
        public IActionResult PendingRestaurants()
        {
            if (_currentAdmin == null) return RedirectToAction("Login");
            return View(_restaurantApplications.Where(r => r.Status == "Pending").ToList());
        }

        [HttpGet]
        public IActionResult ApprovedRestaurants()
        {
            if (_currentAdmin == null) return RedirectToAction("Login");
            var restaurants = HomeController.GetAllRestaurants();
            return View(restaurants?.Where(r => r.IsApproved).ToList() ?? new List<Restaurant>());
        }

        [HttpGet]
        public IActionResult ReviewRestaurant(int id)
        {
            if (_currentAdmin == null) return RedirectToAction("Login");
            var application = _restaurantApplications.FirstOrDefault(r => r.Id == id);
            return application == null ? NotFound() : View(application);
        }

        [HttpPost]
        public IActionResult ApproveRestaurant(int id, string notes)
        {
            var application = _restaurantApplications.FirstOrDefault(r => r.Id == id);
            if (application != null)
            {
                application.Status = "Approved";
                application.ReviewedDate = DateTime.Now;
                application.AdminNotes = notes;
                AddNewRestaurant(application);
            }
            return RedirectToAction("PendingRestaurants");
        }

        [HttpPost]
        public IActionResult ApproveRestaurantJson([FromBody] ApproveRejectRequest request)
        {
            var application = _restaurantApplications.FirstOrDefault(r => r.Id == request.Id);
            if (application == null) return Json(new { success = false, message = "Application not found" });

            application.Status = "Approved";
            application.ReviewedDate = DateTime.Now;
            application.AdminNotes = request.Notes ?? "";
            AddNewRestaurant(application);
            OrderLogger.Instance.Log($"Restaurant '{application.RestaurantName}' approved by admin");
            return Json(new { success = true, message = "Restaurant approved successfully!" });
        }

        [HttpPost]
        public IActionResult RejectRestaurantJson([FromBody] ApproveRejectRequest request)
        {
            var application = _restaurantApplications.FirstOrDefault(r => r.Id == request.Id);
            if (application == null) return Json(new { success = false, message = "Application not found" });

            application.Status = "Rejected";
            application.ReviewedDate = DateTime.Now;
            application.AdminNotes = request.Notes ?? "";
            OrderLogger.Instance.Log($"Restaurant '{application.RestaurantName}' rejected by admin");
            return Json(new { success = true, message = "Restaurant rejected." });
        }

        private void AddNewRestaurant(RestaurantApproval application)
        {
            var newRestaurant = new Restaurant
            {
                Id = Guid.NewGuid().ToString(),
                Name = application.RestaurantName ?? "New Restaurant",
                Category = application.Category ?? "Fast Food",
                DeliveryTime = "30-45 min",
                Rating = 0,
                Reviews = 0,
                Menu = new List<MenuItem>(),
                IsApproved = true,
                OwnerEmail = application.OwnerEmail ?? ""
            };

            if (application.MenuItems != null)
            {
                foreach (var item in application.MenuItems)
                {
                    newRestaurant.Menu.Add(new MenuItem
                    {
                        Id = GenerateNextId(),
                        Name = item.Name ?? "New Item",
                        Price = item.Price,
                        Type = item.Type ?? "burger",
                        Description = item.Description ?? "",
                        Customizations = new List<Customization>()
                    });
                }
            }
            HomeController.AddRestaurant(newRestaurant);
        }

        // ========== DRIVER MANAGEMENT ==========

        [HttpGet]
        public IActionResult PendingDrivers()
        {
            if (_currentAdmin == null) return RedirectToAction("Login");
            return View(_driverApplications.Where(d => d.Status == "Pending").ToList());
        }

        [HttpGet]
        public IActionResult ActiveDrivers()
        {
            if (_currentAdmin == null) return RedirectToAction("Login");
            return View(_activeDrivers);
        }

        [HttpPost]
        public IActionResult ApproveDriverJson([FromBody] ApproveRejectRequest request)
        {
            var application = _driverApplications.FirstOrDefault(d => d.Id == request.Id);
            if (application == null) return Json(new { success = false, message = "Driver application not found" });

            application.Status = "Approved";
            application.ReviewedDate = DateTime.Now;

            _activeDrivers.Add(new DriverInfo
            {
                DriverName = application.DriverName,
                Email = application.Email,
                Phone = application.Phone,
                Status = "Available",
                VehicleType = application.VehicleType,
                Rating = 5.0,
                TotalDeliveries = 0
            });

            OrderLogger.Instance.Log($"Driver '{application.DriverName}' approved by admin");
            return Json(new { success = true, message = "Driver approved successfully!" });
        }

        // ========== SYSTEM SETTINGS ==========

        [HttpGet]
        public IActionResult Settings()
        {
            if (_currentAdmin == null) return RedirectToAction("Login");
            return View(_settings);
        }

        [HttpPost]
        public IActionResult UpdateSettingsJson([FromBody] SystemSettings settings)
        {
            if (settings != null)
            {
                _settings = settings;
                OrderLogger.Instance.Log($"System settings updated - Delivery Fee: R{settings.DeliveryFee}");
            }
            return Json(new { success = true, message = "Settings updated successfully!" });
        }

        [HttpPost]
        public IActionResult AddPromoCode([FromBody] PromoCode promo)
        {
            _settings.GlobalPromoCodes ??= new List<PromoCode>();
            if (promo != null)
            {
                promo.IsActive = true;
                _settings.GlobalPromoCodes.Add(promo);
                OrderLogger.Instance.Log($"New promo code added: {promo.Code}");
            }
            return Json(new { success = true, message = "Promo code added successfully!" });
        }

        // ========== JSON ENDPOINTS ==========

        [HttpGet]
        public IActionResult GetDashboardStatsJson()
        {
            var stats = GetDashboardStatsData();
            return Json(new { success = true, stats = stats });
        }

        [HttpGet]
        public IActionResult GetPendingRestaurants()
        {
            var pending = _restaurantApplications.Where(r => r.Status == "Pending").Select(r => new
            {
                r.Id,
                r.RestaurantName,
                r.OwnerName,
                r.OwnerEmail,
                r.Category,
                r.SubmittedDate,
                MenuCount = r.MenuItems?.Count ?? 0
            });
            return Json(new { success = true, data = pending });
        }

        [HttpGet]
        public IActionResult GetSystemSettings() => Json(new { success = true, settings = _settings });

        [HttpPost]
        public IActionResult ExportDashboardReport()
        {
            try
            {
                var report = GenerateFullReport();
                var filename = $"dashboard_report_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                var directory = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Reports");
                Directory.CreateDirectory(directory);
                var filePath = Path.Combine(directory, filename);
                var json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(filePath, json);
                return Json(new { success = true, filename = filename });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ========== HELPER METHODS ==========

        private object GetDashboardStatsData()
        {
            var weeklyRevenue = HomeController.GetWeeklyRevenueData();
            var categoryData = HomeController.GetOrdersByCategoryData();

            return new
            {
                totalOrders = OrderLogger.Instance.TotalOrders,
                totalRevenue = OrderLogger.Instance.TotalRevenue,
                totalRestaurants = HomeController.GetAllRestaurants()?.Count ?? 0,
                totalDrivers = _activeDrivers.Count,
                todayRevenue = HomeController.GetTodayRevenue(),
                todayOrders = HomeController.GetTodayOrderCount(),
                activeOrders = HomeController.GetActiveOrders()?.Count ?? 0,
                pendingApprovals = _restaurantApplications.Count(r => r.Status == "Pending"),
                weeklyRevenue = weeklyRevenue.Select(w => w.Amount).ToArray(),
                weeklyLabels = weeklyRevenue.Select(w => w.Date.ToString("MM/dd")).ToArray(),
                categoryCounts = categoryData.Select(c => c.Count).ToArray(),
                categoryLabels = categoryData.Select(c => c.Category).ToArray(),
                topRestaurants = GetTopRestaurantsFromOrders()
            };
        }

        private int GenerateNextId()
        {
            var restaurants = HomeController.GetAllRestaurants();
            var allItems = restaurants?.SelectMany(r => r.Menu) ?? new List<MenuItem>();
            return allItems.Any() ? allItems.Max(i => i.Id) + 1 : 37;
        }

        private object GenerateFullReport()
        {
            var orders = HomeController.GetAllOrders() ?? new List<CompletedOrder>();
            return new
            {
                generatedAt = DateTime.Now,
                generatedBy = _currentAdmin?.Username ?? "Unknown",
                totalOrders = orders.Count,
                totalRevenue = orders.Sum(o => o.TotalAmount),
                platformCommissionRate = _settings.CommissionRate,
                platformCommissionAmount = orders.Sum(o => o.TotalAmount) * _settings.CommissionRate,
                netRevenue = orders.Sum(o => o.TotalAmount) * (1 - _settings.CommissionRate),
                activeDrivers = _activeDrivers.Count,
                pendingRestaurants = _restaurantApplications.Count(r => r.Status == "Pending"),
                pendingDrivers = _driverApplications.Count(d => d.Status == "Pending")
            };
        }
    }

    // ========== REQUEST CLASSES ==========
    public class LoginRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

    public class ApproveRejectRequest
    {
        public int Id { get; set; }
        public string? Notes { get; set; }
    }

    public class DeletePromoRequest
    {
        public string? Code { get; set; }
    }

    // ========== LOCAL DRIVER INFO CLASS ==========
    public class DriverInfo
    {
        public string? DriverName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Status { get; set; }
        public string? VehicleType { get; set; }
        public double Rating { get; set; }
        public int TotalDeliveries { get; set; }
    }
}