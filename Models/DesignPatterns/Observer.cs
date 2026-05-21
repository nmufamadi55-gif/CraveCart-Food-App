using System;
using System.Collections.Generic;

namespace CraveCart.Models.DesignPatterns
{
    //  OBSERVER PATTERN
    
    public interface IOrderObserver
    {
        void Update(string message);
        string GetName();
    }

    public class CustomerObserver : IOrderObserver
    {
        public void Update(string message) => Console.WriteLine($"   📱 Customer: {message}");
        public string GetName() => "Customer";
    }

    public class RestaurantObserver : IOrderObserver
    {
        public void Update(string message) => Console.WriteLine($"   🍽️ Restaurant: {message}");
        public string GetName() => "Restaurant";
    }

    public class DriverObserver : IOrderObserver
    {
        public void Update(string message) => Console.WriteLine($"   🚗 Driver: {message}");
        public string GetName() => "Driver";
    }

    public class OrderNotifier
    {
        private List<IOrderObserver> _observers = new List<IOrderObserver>();

        public void Attach(IOrderObserver observer) => _observers.Add(observer);
        public void Notify(string message) => _observers.ForEach(o => o.Update(message));
    }
}