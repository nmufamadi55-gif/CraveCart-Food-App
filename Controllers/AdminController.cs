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
using System.Threading.Tasks;

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
                    Username = "admin@cravecart.com",
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
                _activeDrivers.Add(new DriverInfo
                {
                    DriverName = "Sarah",
                    Email = "sarah@delivery.com",
                    Phone = "0823456789",
                    Status = "Available",
                    VehicleType = "Scooter",
                    Rating = 4.9,
                    TotalDeliveries = 200
                });
                _activeDrivers.Add(new DriverInfo
                {
                    DriverName = "David",
                    Email = "david@delivery.com",
                    Phone = "0834567890",
                    Status = "Available",
                    VehicleType = "Car",
                    Rating = 4.7,
                    TotalDeliveries = 98
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
            return RedirectToAction("Index", "Home");
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

            var weeklyRevenueData = HomeController.GetWeeklyRevenueData();
            var categoryData = HomeController.GetOrdersByCategoryData();

            var weeklyRevenue = weeklyRevenueData.Select(w => new CraveCart.Models.Admin.DailyRevenue
            {
                Date = w.Date,
                Amount = w.Amount,
                OrderCount = w.OrderCount
            }).ToList();

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
        public IActionResult ApproveRestaurantJson([FromBody] ApproveRejectRequest request)
        {
            var application = _restaurantApplications.FirstOrDefault(r => r.Id == request.Id);
            if (application == null) return Json(new { success = false, message = "Application not found" });

            application.Status = "Approved";
            application.ReviewedDate = DateTime.Now;
            application.AdminNotes = request.Notes ?? "";
            AddNewRestaurant(application);

            // Observer Pattern: Notify Restaurant
            var notifier = new OrderNotifier();
            notifier.Attach(new RestaurantObserver());
            notifier.Notify($"✅ Restaurant '{application.RestaurantName}' has been approved! You can now start accepting orders.");

            // Singleton Logger: Record approval
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

            // Observer Pattern: Notify Restaurant of rejection
            var notifier = new OrderNotifier();
            notifier.Attach(new RestaurantObserver());
            notifier.Notify($"❌ Restaurant '{application.RestaurantName}' application was not approved. Reason: {request.Notes}");

            OrderLogger.Instance.Log($"Restaurant '{application.RestaurantName}' rejected by admin");
            return Json(new { success = true, message = "Restaurant rejected." });
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

            // Register driver in DriverController system
            DriverController.RegisterDriver(new DriverUser
            {
                Id = _activeDrivers.Count + 100,
                Name = application.DriverName,
                Email = application.Email,
                Phone = application.Phone,
                VehicleType = application.VehicleType,
                VehiclePlate = application.VehiclePlate,
                CurrentStatus = "Available",
                Rating = 5.0,
                TotalDeliveries = 0,
                IsActive = true
            });

            // Observer Pattern: Notify Driver
            var notifier = new OrderNotifier();
            notifier.Attach(new DriverObserver());
            notifier.Notify($"✅ Driver '{application.DriverName}' has been approved! You can now start accepting deliveries.");

            OrderLogger.Instance.Log($"Driver '{application.DriverName}' approved by admin");
            return Json(new { success = true, message = "Driver approved successfully!" });
        }

        [HttpPost]
        public IActionResult RejectDriverJson([FromBody] ApproveRejectRequest request)
        {
            var application = _driverApplications.FirstOrDefault(d => d.Id == request.Id);
            if (application == null) return Json(new { success = false, message = "Driver application not found" });

            application.Status = "Rejected";
            application.ReviewedDate = DateTime.Now;
            application.AdminNotes = request.Notes ?? "";

            // Observer Pattern: Notify Driver of rejection
            var notifier = new OrderNotifier();
            notifier.Attach(new DriverObserver());
            notifier.Notify($"❌ Driver '{application.DriverName}' application was not approved. Reason: {request.Notes}");

            OrderLogger.Instance.Log($"Driver '{application.DriverName}' rejected by admin");
            return Json(new { success = true, message = "Driver rejected." });
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

            // Register restaurant in RestaurantController system
            RestaurantController.RegisterRestaurant(newRestaurant);
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

        [HttpGet]
        public IActionResult GetDriverDetails(string email)
        {
            var driver = _activeDrivers.FirstOrDefault(d => d.Email == email);
            if (driver == null) return Json(new { success = false });

            return Json(new
            {
                success = true,
                driver = new
                {
                    driver.DriverName,
                    driver.Email,
                    driver.Phone,
                    driver.VehicleType,
                    driver.Status,
                    driver.Rating,
                    driver.TotalDeliveries
                }
            });
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
                OrderLogger.Instance.Log($"System settings updated - Delivery Fee: R{settings.DeliveryFee}, Commission: {settings.CommissionRate * 100}%");
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
                OrderLogger.Instance.Log($"New promo code added: {promo.Code} - {promo.DiscountPercent}% off");
            }
            return Json(new { success = true, message = "Promo code added successfully!" });
        }

        [HttpPost]
        public IActionResult DeletePromoCode([FromBody] DeletePromoRequest request)
        {
            if (request?.Code != null && _settings.GlobalPromoCodes != null)
            {
                var promo = _settings.GlobalPromoCodes.FirstOrDefault(p => p.Code == request.Code);
                if (promo != null)
                {
                    _settings.GlobalPromoCodes.Remove(promo);
                    OrderLogger.Instance.Log($"Promo code deleted: {request.Code}");
                    return Json(new { success = true, message = "Promo code deleted!" });
                }
            }
            return Json(new { success = false, message = "Promo code not found" });
        }

        // ========== REPORTS ==========

        [HttpGet]
        public IActionResult Reports(DateTime? startDate, DateTime? endDate)
        {
            if (_currentAdmin == null) return RedirectToAction("Login");

            var start = startDate ?? DateTime.Now.AddDays(-30);
            var end = endDate ?? DateTime.Now;

            var report = GenerateSalesReport(start, end);
            return View(report);
        }

        [HttpGet]
        public IActionResult GetSalesReport(DateTime startDate, DateTime endDate)
        {
            var report = GenerateSalesReport(startDate, endDate);
            return Json(new { success = true, report = report });
        }

        private SalesReport GenerateSalesReport(DateTime start, DateTime end)
        {
            var orders = HomeController.GetAllOrders()?.Where(o => o.OrderDate >= start && o.OrderDate <= end).ToList() ?? new List<CompletedOrder>();

            return new SalesReport
            {
                StartDate = start,
                EndDate = end,
                TotalOrders = orders.Count,
                GrossRevenue = orders.Sum(o => o.TotalAmount),
                PlatformCommission = orders.Sum(o => o.TotalAmount) * _settings.CommissionRate,
                NetRevenue = orders.Sum(o => o.TotalAmount) * (1 - _settings.CommissionRate),
                NewCustomers = 45,
                NewRestaurants = _restaurantApplications.Count(r => r.Status == "Approved" && r.ReviewedDate >= start && r.ReviewedDate <= end),
                OrdersByDay = GetOrdersByDay(start, end)
            };
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
        public IActionResult GetPendingDrivers()
        {
            var pending = _driverApplications.Where(d => d.Status == "Pending").Select(d => new
            {
                d.Id,
                d.DriverName,
                d.Email,
                d.Phone,
                d.VehicleType,
                d.SubmittedDate,
                d.BackgroundCheckScore
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
                OrderLogger.Instance.Log($"Dashboard report exported: {filename}");
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
            var directory = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Reports");
            var filePath = Path.Combine(directory, filename);

            if (System.IO.File.Exists(filePath))
            {
                var bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "application/json", filename);
            }
            return NotFound();
        }

        [HttpGet]
        public IActionResult GetSaveFiles()
        {
            try
            {
                var directory = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Reports");
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var files = Directory.GetFiles(directory, "*.json")
                    .Select(f => new
                    {
                        name = Path.GetFileName(f),
                        date = System.IO.File.GetCreationTime(f).ToString("yyyy-MM-dd HH:mm:ss"),
                        size = new FileInfo(f).Length
                    })
                    .ToList();

                return Json(new { success = true, files = files });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, files = new List<object>() });
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

        private List<OrderSummary> GetOrdersByDay(DateTime start, DateTime end)
        {
            var result = new List<OrderSummary>();
            var orders = HomeController.GetAllOrders() ?? new List<CompletedOrder>();

            for (var date = start; date <= end; date = date.AddDays(1))
            {
                var dayOrders = orders.Where(o => o.OrderDate.Date == date.Date).ToList();
                result.Add(new OrderSummary
                {
                    Date = date,
                    OrderCount = dayOrders.Count,
                    Revenue = dayOrders.Sum(o => o.TotalAmount),
                    AverageOrderValue = dayOrders.Any() ? dayOrders.Average(o => o.TotalAmount) : 0
                });
            }
            return result;
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
            var restaurants = HomeController.GetAllRestaurants();

            return new
            {
                generatedAt = DateTime.Now,
                generatedBy = _currentAdmin?.Username ?? "Unknown",
                totalOrders = orders.Count,
                totalRevenue = orders.Sum(o => o.TotalAmount),
                platformCommissionRate = _settings.CommissionRate,
                platformCommissionAmount = orders.Sum(o => o.TotalAmount) * _settings.CommissionRate,
                netRevenue = orders.Sum(o => o.TotalAmount) * (1 - _settings.CommissionRate),
                restaurants = restaurants?.Select(r => new { r.Name, r.Category, r.Rating, menuCount = r.Menu.Count }),
                activeDrivers = _activeDrivers.Select(d => new { d.DriverName, d.Status, d.Rating, d.TotalDeliveries }),
                pendingRestaurants = _restaurantApplications.Count(r => r.Status == "Pending"),
                pendingDrivers = _driverApplications.Count(d => d.Status == "Pending"),
                systemSettings = new
                {
                    _settings.DeliveryFee,
                    _settings.MinOrderAmount,
                    _settings.LoyaltyPointsMultiplier,
                    _settings.CommissionRate,
                    _settings.MaintenanceMode,
                    activePromoCodes = _settings.GlobalPromoCodes?.Where(p => p.IsActive).Count() ?? 0
                }
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

    // ========== SALES REPORT CLASS ==========
    public class SalesReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalOrders { get; set; }
        public decimal GrossRevenue { get; set; }
        public decimal PlatformCommission { get; set; }
        public decimal NetRevenue { get; set; }
        public int NewCustomers { get; set; }
        public int NewRestaurants { get; set; }
        public List<OrderSummary>? OrdersByDay { get; set; }
    }

    public class OrderSummary
    {
        public DateTime Date { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal AverageOrderValue { get; set; }
    }
}