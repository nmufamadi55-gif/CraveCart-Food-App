using Microsoft.AspNetCore.Mvc;
using CraveCart.Models.DesignPatterns;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraveCart.Controllers
{
    public class DriverController : Controller
    {
        private static DriverUser? _currentDriver;
        private static List<DriverUser> _allDrivers = new();
        private static List<DeliveryTask> _availableTasks = new();
        private static List<DeliveryTask> _assignedTasks = new();

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
                    CurrentStatus = "Available"
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
                    CurrentStatus = "Available"
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
                return RedirectToAction("Dashboard");
            }
            ViewBag.Error = "Invalid credentials";
            return View();
        }

        [HttpPost]
        public IActionResult Logout()
        {
            if (_currentDriver != null)
                _currentDriver.CurrentStatus = "Offline";
            _currentDriver = null;
            return RedirectToAction("Login");
        }

        // ========== DASHBOARD ==========

        [HttpGet]
        public IActionResult Dashboard()
        {
            if (_currentDriver == null) return RedirectToAction("Login");

            ViewBag.Driver = _currentDriver;
            ViewBag.AvailableTasks = _availableTasks.Count;
            ViewBag.ActiveTask = _assignedTasks.FirstOrDefault(t => t.Status != "Completed");
            return View();
        }

        [HttpGet]
        public IActionResult AvailableDeliveries()
        {
            if (_currentDriver == null) return RedirectToAction("Login");
            return View(_availableTasks);
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
                    _currentDriver.CurrentStatus = "Busy";

                var notifier = new OrderNotifier();
                notifier.Attach(new CustomerObserver());
                notifier.Attach(new RestaurantObserver());
                notifier.Notify($"Driver {_currentDriver?.Name} assigned to your order!");
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
            }
            return RedirectToAction("ActiveDelivery");
        }

        [HttpPost]
        public IActionResult MarkDelivered(int taskId)
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
                    _currentDriver.CurrentStatus = "Online";
                }
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
            return View(completedTasks);
        }

        [HttpGet]
        public IActionResult Profile()
        {
            if (_currentDriver == null) return RedirectToAction("Login");
            return View(_currentDriver);
        }

        [HttpPost]
        public IActionResult UpdateStatus(string status)
        {
            if (_currentDriver != null)
            {
                _currentDriver.CurrentStatus = status;
                OrderLogger.Instance.Log($"Driver {_currentDriver.Name} is now {status}");
            }
            return Json(new { success = true, status = status });
        }
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