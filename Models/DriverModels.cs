using System;
using System.Collections.Generic;

namespace CraveCart.Models.Driver
{
    public class DriverUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public string LicenseNumber { get; set; }
        public string VehicleType { get; set; }
        public string VehiclePlate { get; set; }
        public bool IsActive { get; set; }
        public double Rating { get; set; }
        public int TotalDeliveries { get; set; }
        public decimal TotalEarnings { get; set; }
        public string CurrentStatus { get; set; } // Online, Offline, Busy
        public Location CurrentLocation { get; set; }
    }

    public class Location
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime LastUpdate { get; set; }
    }

    public class DeliveryTask
    {
        public int TaskId { get; set; }
        public int OrderId { get; set; }
        public string RestaurantName { get; set; }
        public string RestaurantAddress { get; set; }
        public Location RestaurantLocation { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public Location CustomerLocation { get; set; }
        public string CustomerPhone { get; set; }
        public DateTime AssignedTime { get; set; }
        public DateTime? PickupTime { get; set; }
        public DateTime? DeliveryTime { get; set; }
        public string Status { get; set; } // Assigned, Pickup, Delivered, Completed
        public decimal DeliveryFee { get; set; }
        public decimal Tip { get; set; }
        public double Distance { get; set; } // in km
        public int EstimatedMinutes { get; set; }
    }

    public class DriverStats
    {
        public int TodayDeliveries { get; set; }
        public decimal TodayEarnings { get; set; }
        public int WeeklyDeliveries { get; set; }
        public decimal WeeklyEarnings { get; set; }
        public double AverageRating { get; set; }
        public int TotalDeliveries { get; set; }
        public decimal TotalEarnings { get; set; }
        public double OnTimePercentage { get; set; }
        public List<EarningBreakdown> EarningsByDay { get; set; }
    }

    public class EarningBreakdown
    {
        public DateTime Date { get; set; }
        public int Deliveries { get; set; }
        public decimal Amount { get; set; }
    }

    public class DeliveryHistory
    {
        public int OrderId { get; set; }
        public string RestaurantName { get; set; }
        public string CustomerAddress { get; set; }
        public DateTime DeliveryTime { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Tip { get; set; }
        public double Rating { get; set; }
        public string Feedback { get; set; }
    }
}