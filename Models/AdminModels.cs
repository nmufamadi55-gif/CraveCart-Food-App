using System;
using System.Collections.Generic;

namespace CraveCart.Models.Admin
{
    // Admin User Model
    public class AdminUser
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } // In production, hash this!
        public string Email { get; set; }
        public string Role { get; set; } // SuperAdmin, Manager, Analyst
        public DateTime CreatedAt { get; set; }
        public DateTime LastLogin { get; set; }
    }

    // Dashboard Statistics
    public class DashboardStats
    {
        public int TotalOrders { get; set; }
        public int TotalUsers { get; set; }
        public int TotalRestaurants { get; set; }
        public int TotalDrivers { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public int ActiveOrders { get; set; }
        public int PendingApprovals { get; set; }

        // Charts data
        public List<DailyRevenue> WeeklyRevenue { get; set; }
        public List<OrderCount> OrdersByCategory { get; set; }
        public List<TopRestaurant> TopRestaurants { get; set; }
        public int TodayOrders { get; internal set; }
    }

    public class DailyRevenue
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public int OrderCount { get; set; }
    }

    public class OrderCount
    {
        public string Category { get; set; }
        public int Count { get; set; }
    }

    public class TopRestaurant
    {
        public string Name { get; set; }
        public int Orders { get; set; }
        public decimal Revenue { get; set; }
        public double Rating { get; set; }
    }

    // Restaurant Approval
    public class RestaurantApproval
    {
        public int Id { get; set; }
        public string RestaurantName { get; set; }
        public string OwnerName { get; set; }
        public string OwnerEmail { get; set; }
        public string Category { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string LicenseNumber { get; set; }
        public string Status { get; set; } // Pending, Approved, Rejected
        public DateTime SubmittedDate { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public string AdminNotes { get; set; }
        public List<MenuItemRequest> MenuItems { get; set; }
    }

    public class MenuItemRequest
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
    }

    // Driver Approval
    public class DriverApproval
    {
        public int Id { get; set; }
        public string DriverName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string LicenseNumber { get; set; }
        public string VehicleType { get; set; }
        public string VehiclePlate { get; set; }
        public string Status { get; set; } // Pending, Approved, Rejected
        public DateTime SubmittedDate { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public double BackgroundCheckScore { get; set; }
        public string AdminNotes { get; internal set; }
    }

    // System Settings
    public class SystemSettings
    {
        public decimal DeliveryFee { get; set; } = 25;
        public decimal MinOrderAmount { get; set; } = 0;
        public int LoyaltyPointsMultiplier { get; set; } = 10;
        public decimal CommissionRate { get; set; } = 0.15m; // 15% platform fee
        public bool MaintenanceMode { get; set; } = false;
        public string MaintenanceMessage { get; set; }
        public List<PromoCode> GlobalPromoCodes { get; set; }
    }

    public class PromoCode
    {
        public string Code { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal MaxDiscount { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public int UsageLimit { get; set; }
        public int UsedCount { get; set; }
        public bool IsActive { get; set; }
    }

    // Reports
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
        public List<OrderSummary> OrdersByDay { get; set; }
    }

    public class OrderSummary
    {
        public DateTime Date { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal AverageOrderValue { get; set; }
    }


}