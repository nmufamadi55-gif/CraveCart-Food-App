using System;
using System.Collections.Generic;

namespace CraveCart.Models.DesignPatterns
{
    // SINGLETON PATTERN
    public sealed class OrderLogger
    {
        private static OrderLogger _instance;
        private static readonly object _lock = new object();

        public List<string> Logs { get; private set; }
        public int TotalOrders { get; private set; }
        public decimal TotalRevenue { get; private set; }

        private OrderLogger()
        {
            Logs = new List<string>();
            TotalOrders = 0;
            TotalRevenue = 0;
        }

        public static OrderLogger Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new OrderLogger();
                    return _instance;
                }
            }
        }

        public void Log(string message)
        {
            Logs.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
            System.Diagnostics.Debug.WriteLine(message);
        }

        public void RecordOrder(decimal amount, string restaurant, string items)
        {
            TotalOrders++;
            TotalRevenue += amount;
            Log($"Order #{TotalOrders}: {items} from {restaurant} - R{amount:F2}");
        }

        public string GetSummary()
        {
            return $"Total Orders: {TotalOrders} | Revenue: R{TotalRevenue:F2}";
        }
    }
}