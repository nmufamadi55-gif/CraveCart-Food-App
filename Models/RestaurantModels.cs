using CraveCart.Models.AbstractFactory.Interfaces;
using System;
using System.Collections.Generic;

namespace CraveCart.Models
{
    public class Restaurant
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string DeliveryTime { get; set; }
        public string ImageUrl { get; set; }
        public string CoverColor { get; set; }
        public string ImageIcon { get; set; }
        public IRestaurantFactory Factory { get; set; }
        public double Rating { get; set; }
        public int Reviews { get; set; }
        public bool IsApproved { get; set; } = true;
        public string OwnerEmail { get; set; }
        public bool IsAcceptingOrders { get; set; } = true;
        public TimeSpan OpenTime { get; set; } = new TimeSpan(9, 0, 0);
        public TimeSpan CloseTime { get; set; } = new TimeSpan(22, 0, 0);
        public List<MenuItem> Menu { get; set; }

        public Restaurant()
        {
            Menu = new List<MenuItem>();
            Rating = 4.5;
            Reviews = 1000;
        }
    }

    public class MenuItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Type { get; set; }
        public string Icon { get; set; } 
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public bool IsPopular { get; set; }
        public int RestaurantId { get; set; }
        public List<Customization> Customizations { get; set; }

        public MenuItem()
        {
            Customizations = new List<Customization>();
        }
    }

    public class Customization
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Icon { get; set; }
    }

    public class CartItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public MenuItem Item { get; set; }
        public List<string> Extras { get; set; } = new List<string>();
        public decimal TotalPrice { get; set; }
        public string ItemName { get; set; }
        public string ItemIcon { get; set; }
    }

    public class MenuManagement
    {
        public List<MenuItem> Items { get; set; }
        public List<CategoryInfo> Categories { get; set; }
        public bool IsAcceptingOrders { get; set; }
        public string ClosedReason { get; set; }
    }

    public class CategoryInfo
    {
        public string Name { get; set; }
        public int ItemCount { get; set; }
        public bool IsAvailable { get; set; }
    }
}