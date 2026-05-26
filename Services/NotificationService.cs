using CraveCart.Models.DesignPatterns;
using System.Collections.Generic;

namespace CraveCart.Services
{
    public static class NotificationService
    {
        private static OrderNotifier _globalNotifier = new OrderNotifier();

        static NotificationService()
        {
            // Attach default observers
            _globalNotifier.Attach(new CustomerObserver());
            _globalNotifier.Attach(new RestaurantObserver());
            _globalNotifier.Attach(new DriverObserver());
        }

        public static void NotifyAll(string message)
        {
            _globalNotifier.Notify(message);
        }

        public static void NotifyCustomer(string message)
        {
            Console.WriteLine($"   📱 Customer: {message}");
        }

        public static void NotifyRestaurant(string message)
        {
            Console.WriteLine($"   🍽️ Restaurant: {message}");
        }

        public static void NotifyDriver(string message)
        {
            Console.WriteLine($"   🚗 Driver: {message}");
        }
    }
}