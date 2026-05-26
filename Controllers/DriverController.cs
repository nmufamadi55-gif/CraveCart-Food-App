using Microsoft.AspNetCore.Mvc;
using CraveCart.Models.DesignPatterns;
using CraveCart.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CraveCart.Controllers
{
    public class DriverController : Controller
    {
        private static DriverUser? _currentDriver;
        private static List<DriverUser> _allDrivers = new();
        private static List<DeliveryTask> _availableTasks = new();
        private static List<DeliveryTask> _assignedTasks = new();

        // Track which driver is assigned to which order
        private static Dictionary<int, int> _orderDriverMap = new();
        private static Dictionary<int, int> _orderDriverEta = new();

        public DriverController()
        {
            InitializeDrivers();
        }

        private void InitializeDrivers()
        {
            if (_allDrivers.Count == 0)
            {
                _allDrivers.Add(new DriverUser
                {
                    Id = 1,
                    Name = "Michael",
                    Email = "michael@delivery.com",
                    Phone = "0812345678",
                    Password = "driver123",
                    VehicleType = "Motorcycle",
                    VehiclePlate = "ABC123",
                    IsActive = true,
                    Rating = 4.8,
                    TotalDeliveries = 150,
                    TotalEarnings = 12500m,
                    CurrentStatus = "Available",
                    EstimatedMinutes = 15
                });
                _allDrivers.Add(new DriverUser
                {
                    Id = 2,
                    Name = "Sarah",
                    Email = "sarah@delivery.com",
                    Phone = "0823456789",
                    Password = "driver123",
                    VehicleType = "Scooter",
                    VehiclePlate = "XYZ789",
                    IsActive = true,
                    Rating = 4.9,
                    TotalDeliveries = 200,
                    TotalEarnings = 16800m,
                    CurrentStatus = "Available",
                    EstimatedMinutes = 12
                });
                _allDrivers.Add(new DriverUser
                {
                    Id = 3,
                    Name = "David",
                    Email = "david@delivery.com",
                    Phone = "0834567890",
                    Password = "driver123",
                    VehicleType = "Car",
                    VehiclePlate = "DEF456",
                    IsActive = true,
                    Rating = 4.7,
                    TotalDeliveries = 98,
                    TotalEarnings = 8200m,
                    CurrentStatus = "Available",
                    EstimatedMinutes = 20
                });
            }

            if (_availableTasks.Count == 0)
            {
                _availableTasks.Add(new DeliveryTask
                {
                    TaskId = 1,
                    OrderId = 2001,
                    RestaurantName = "Burger King",
                    RestaurantAddress = "123 Main St",
                    CustomerName = "John Doe",
                    CustomerAddress = "456 Oak Ave",
                    CustomerPhone = "0831234567",
                    DeliveryFee = 25,
                    Distance = 3.5,
                    EstimatedMinutes = 15,
                    Status = "Available"
                });
                _availableTasks.Add(new DeliveryTask
                {
                    TaskId = 2,
                    OrderId = 2002,
                    RestaurantName = "Sweetgreen",
                    RestaurantAddress = "789 Pine St",
                    CustomerName = "Jane Smith",
                    CustomerAddress = "321 Elm Blvd",
                    CustomerPhone = "0847654321",
                    DeliveryFee = 30,
                    Distance = 2.8,
                    EstimatedMinutes = 12,
                    Status = "Available"
                });
            }
        }

        // ========== AUTHENTICATION ==========
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var driver = _allDrivers.FirstOrDefault(d => d.Email == email && d.Password == password);
            if (driver != null)
            {
                _currentDriver = driver;
                _currentDriver.CurrentStatus = "Online";
                OrderLogger.Instance.Log($"Driver '{driver.Name}' logged in");
                return RedirectToAction("Dashboard");
            }
            ViewBag.Error = "Invalid credentials";
            return View();
        }

        [HttpPost]
        public IActionResult Logout()
        {
            if (_currentDriver != null)
            {
                OrderLogger.Instance.Log($"Driver '{_currentDriver.Name}' logged out");
                _currentDriver.CurrentStatus = "Offline";
                _currentDriver = null;
            }
            return RedirectToAction("Index", "Home");
        }

        // ========== DASHBOARD ==========
        [HttpGet]
        public IActionResult Dashboard()
        {
            if (_currentDriver == null) return RedirectToAction("Login");

            ViewBag.DriverName = _currentDriver.Name;
            ViewBag.DriverStatus = _currentDriver.CurrentStatus;
            ViewBag.DriverRating = _currentDriver.Rating;
            ViewBag.TotalEarnings = _currentDriver.TotalEarnings;
            ViewBag.TotalDeliveries = _currentDriver.TotalDeliveries;
            ViewBag.VehicleType = _currentDriver.VehicleType;
            ViewBag.AvailableTasks = _availableTasks.Count;
            ViewBag.ActiveTask = _assignedTasks.FirstOrDefault(t => t.Status != "Completed");
            return View();
        }

        [HttpGet]
        public IActionResult AvailableDeliveries()
        {
            if (_currentDriver == null) return RedirectToAction("Login");

            var nearbyTasks = _availableTasks.OrderBy(t => t.Distance).ToList();
            return View(nearbyTasks);
        }

        [HttpPost]
        public IActionResult AcceptDelivery(int taskId)
        {
            var task = _availableTasks.FirstOrDefault(t => t.TaskId == taskId);
            if (task != null)
            {
                _availableTasks.Remove(task);
                task.Status = "Assigned";
                task.AssignedTime = DateTime.Now;
                _assignedTasks.Add(task);

                if (_currentDriver != null)
                {
                    _currentDriver.CurrentStatus = "Busy";

                    // Map driver to order
                    _orderDriverMap[task.OrderId] = _currentDriver.Id;
                    _orderDriverEta[task.OrderId] = task.EstimatedMinutes;

                    // Sync to customer
                    HomeController.UpdateOrderDriver(task.OrderId, _currentDriver.Name, task.EstimatedMinutes);
                    HomeController.UpdateOrderStatus(task.OrderId, "DriverAssigned");
                }

                var notifier = new OrderNotifier();
                notifier.Attach(new CustomerObserver());
                notifier.Attach(new RestaurantObserver());
                notifier.Notify($"🚗 Driver {_currentDriver?.Name} has been assigned to your order!");
                notifier.Notify($"🚗 Driver {_currentDriver?.Name} is on the way to pickup order #{task.OrderId}");

                OrderLogger.Instance.Log($"Driver '{_currentDriver?.Name}' assigned to order #{task.OrderId} - Delivery Fee: R{task.DeliveryFee}");
            }
            return RedirectToAction("ActiveDelivery");
        }

        [HttpGet]
        public IActionResult ActiveDelivery()
        {
            if (_currentDriver == null) return RedirectToAction("Login");

            var activeTask = _assignedTasks.FirstOrDefault(t => t.Status != "Completed");
            if (activeTask == null)
                return RedirectToAction("Dashboard");

            return View(activeTask);
        }

        [HttpPost]
        public IActionResult MarkPickedUp(int taskId)
        {
            var task = _assignedTasks.FirstOrDefault(t => t.TaskId == taskId);
            if (task != null)
            {
                task.Status = "Pickup";
                task.PickupTime = DateTime.Now;

                var notifier = new OrderNotifier();
                notifier.Attach(new CustomerObserver());
                notifier.Attach(new RestaurantObserver());
                notifier.Notify($"📦 Driver has picked up your order #{task.OrderId}! En route to delivery!");
                notifier.Notify($"🚚 Order #{task.OrderId} picked up by driver {_currentDriver?.Name}, out for delivery");

                // Sync to customer
                HomeController.UpdateOrderStatus(task.OrderId, "OutForDelivery");

                OrderLogger.Instance.Log($"Driver '{_currentDriver?.Name}' picked up order #{task.OrderId}");
            }
            return RedirectToAction("ActiveDelivery");
        }

        [HttpPost]
        public IActionResult MarkDelivered(int taskId, double rating = 5)
        {
            var task = _assignedTasks.FirstOrDefault(t => t.TaskId == taskId);
            if (task != null)
            {
                task.Status = "Completed";
                task.DeliveryTime = DateTime.Now;

                if (_currentDriver != null)
                {
                    _currentDriver.TotalDeliveries++;
                    _currentDriver.TotalEarnings += task.DeliveryFee;
                    _currentDriver.Rating = (_currentDriver.Rating * (_currentDriver.TotalDeliveries - 1) + rating)
                                          / _currentDriver.TotalDeliveries;
                    _currentDriver.CurrentStatus = "Online";
                }

                var notifier = new OrderNotifier();
                notifier.Attach(new CustomerObserver());
                notifier.Attach(new RestaurantObserver());
                notifier.Notify($"🎉 Order #{task.OrderId} has been delivered! Thank you for using CraveCart!");
                notifier.Notify($"✅ Order #{task.OrderId} marked as delivered by driver {_currentDriver?.Name}");

                // Sync to customer - final status
                HomeController.UpdateOrderStatus(task.OrderId, "Delivered");
                HomeController.CompleteOrder(task.OrderId);

                OrderLogger.Instance.Log($"Order #{task.OrderId} delivered by '{_currentDriver?.Name}'. Rating: {rating}⭐. Delivery Fee: R{task.DeliveryFee}");
            }
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public IActionResult Earnings()
        {
            if (_currentDriver == null) return RedirectToAction("Login");

            var completedTasks = _assignedTasks.Where(t => t.Status == "Completed").ToList();
            ViewBag.TotalDeliveries = completedTasks.Count;
            ViewBag.TotalEarnings = completedTasks.Sum(t => t.DeliveryFee);
            ViewBag.AveragePerDelivery = completedTasks.Any() ? completedTasks.Average(t => t.DeliveryFee) : 0;
            ViewBag.DriverName = _currentDriver.Name;
            return View(completedTasks);
        }

        // ========== DELIVERY HISTORY ==========
        [HttpGet]
        public IActionResult DeliveryHistory()
        {
            if (_currentDriver == null) return RedirectToAction("Login");

            var completedDeliveries = _assignedTasks
                .Where(t => t.Status == "Completed" || t.Status == "Delivered")
                .OrderByDescending(t => t.DeliveryTime)
                .ToList();

            ViewBag.DriverName = _currentDriver.Name;
            ViewBag.DriverStatus = _currentDriver.CurrentStatus;
            ViewBag.DriverRating = _currentDriver.Rating;
            ViewBag.TotalEarnings = _currentDriver.TotalEarnings;
            ViewBag.TotalDeliveries = _currentDriver.TotalDeliveries;

            return View(completedDeliveries);
        }

        [HttpGet]
        public IActionResult Profile()
        {
            if (_currentDriver == null) return RedirectToAction("Login");
            ViewBag.DriverName = _currentDriver.Name;
            ViewBag.DriverStatus = _currentDriver.CurrentStatus;
            return View(_currentDriver);
        }

        [HttpPost]
        public IActionResult UpdateProfile([FromBody] DriverUser updatedProfile)
        {
            if (_currentDriver != null && updatedProfile != null)
            {
                _currentDriver.Phone = updatedProfile.Phone;
                _currentDriver.VehicleType = updatedProfile.VehicleType;
                _currentDriver.VehiclePlate = updatedProfile.VehiclePlate;

                OrderLogger.Instance.Log($"Driver '{_currentDriver.Name}' updated profile");
                return Json(new { success = true, message = "Profile updated successfully!" });
            }
            return Json(new { success = false, message = "Error updating profile" });
        }

        [HttpPost]
        public IActionResult UpdatePassword([FromBody] PasswordUpdateRequest request)
        {
            if (_currentDriver != null && request != null)
            {
                if (request.CurrentPassword == "driver123")
                {
                    _currentDriver.Password = request.NewPassword;
                    OrderLogger.Instance.Log($"Driver '{_currentDriver.Name}' changed password");
                    return Json(new { success = true, message = "Password updated successfully!" });
                }
                return Json(new { success = false, message = "Current password is incorrect" });
            }
            return Json(new { success = false, message = "Error updating password" });
        }

        [HttpPost]
        public IActionResult UpdateStatus(string status)
        {
            if (_currentDriver != null)
            {
                _currentDriver.CurrentStatus = status;
                OrderLogger.Instance.Log($"Driver '{_currentDriver.Name}' changed status to {status}");
            }
            return Json(new { success = true, status = status });
        }

        // ========== STATIC METHODS FOR CROSS-SYSTEM INTEGRATION ==========
        public static DriverUser? AssignAvailableDriver(string restaurantName)
        {
            var availableDriver = _allDrivers.FirstOrDefault(d => d.CurrentStatus == "Available" && d.IsActive);
            if (availableDriver != null)
            {
                availableDriver.CurrentStatus = "Busy";

                var orderId = OrderLogger.Instance.TotalOrders + 1;
                var deliveryTask = new DeliveryTask
                {
                    TaskId = _assignedTasks.Count + 1,
                    OrderId = orderId,
                    RestaurantName = restaurantName,
                    RestaurantAddress = "Restaurant Address",
                    CustomerName = "Customer",
                    CustomerAddress = "Customer Address",
                    CustomerPhone = "0812345678",
                    DeliveryFee = 25,
                    Distance = 3.5,
                    EstimatedMinutes = availableDriver.EstimatedMinutes ?? 20,
                    Status = "Assigned",
                    AssignedTime = DateTime.Now
                };
                _assignedTasks.Add(deliveryTask);

                // Map driver to order
                _orderDriverMap[orderId] = availableDriver.Id;
                _orderDriverEta[orderId] = availableDriver.EstimatedMinutes ?? 20;

                Console.WriteLine($"🚗 Driver System: {availableDriver.Name} assigned to order #{orderId}");

                var notifier = new OrderNotifier();
                notifier.Attach(new CustomerObserver());
                notifier.Notify($"🚗 Driver {availableDriver.Name} has been assigned to your order!");

                OrderLogger.Instance.Log($"Driver '{availableDriver.Name}' automatically assigned to order #{orderId}");

                return availableDriver;
            }

            Console.WriteLine($"🚗 Driver System: No drivers available for {restaurantName}");
            return null;
        }

        public static void NotifyNewReadyOrder(int orderId, string restaurantName)
        {
            var newTask = new DeliveryTask
            {
                TaskId = _availableTasks.Count + 1,
                OrderId = orderId,
                RestaurantName = restaurantName,
                Status = "Available",
                DeliveryFee = 25,
                EstimatedMinutes = 20
            };
            _availableTasks.Add(newTask);
            Console.WriteLine($"📦 New delivery task created for order #{orderId} from {restaurantName}");
        }

        public static DriverUser? GetDriverForOrder(int orderId)
        {
            if (!_orderDriverMap.ContainsKey(orderId)) return null;

            var driverId = _orderDriverMap[orderId];
            var driver = _allDrivers.FirstOrDefault(d => d.Id == driverId);
            if (driver != null && _orderDriverEta.ContainsKey(orderId))
            {
                driver.EstimatedMinutes = _orderDriverEta[orderId];
            }
            return driver;
        }

        public static void RegisterDriver(DriverUser driver)
        {
            if (!_allDrivers.Any(d => d.Email == driver.Email))
            {
                driver.Id = _allDrivers.Any() ? _allDrivers.Max(d => d.Id) + 1 : 1;
                _allDrivers.Add(driver);
                OrderLogger.Instance.Log($"New driver registered: '{driver.Name}' - {driver.VehicleType}");
            }
        }

        public static List<DriverUser> GetAllDrivers() => _allDrivers;
        public static List<DeliveryTask> GetAvailableTasks() => _availableTasks;
    }

    // ========== REQUEST CLASSES ==========
    public class PasswordUpdateRequest
    {
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }

    // ========== MODEL CLASSES ==========
    public class DriverUser
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Password { get; set; }
        public string? VehicleType { get; set; }
        public string? VehiclePlate { get; set; }
        public bool IsActive { get; set; }
        public double Rating { get; set; }
        public int TotalDeliveries { get; set; }
        public decimal TotalEarnings { get; set; }
        public string? CurrentStatus { get; set; }
        public int? EstimatedMinutes { get; set; }
    }

    public class DeliveryTask
    {
        public int TaskId { get; set; }
        public int OrderId { get; set; }
        public string? RestaurantName { get; set; }
        public string? RestaurantAddress { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerPhone { get; set; }
        public DateTime AssignedTime { get; set; }
        public DateTime? PickupTime { get; set; }
        public DateTime? DeliveryTime { get; set; }
        public string? Status { get; set; }
        public decimal DeliveryFee { get; set; }
        public double Distance { get; set; }
        public int EstimatedMinutes { get; set; }
    }
}