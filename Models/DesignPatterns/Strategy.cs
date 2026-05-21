using System;

namespace CraveCart.Models.DesignPatterns
{
    
    //  STRATEGY PATTERN
  
    public interface IPaymentStrategy
    {
        string Pay(decimal amount);
        string GetName();
    }

    public class CreditCardPayment : IPaymentStrategy
    {
        public string Pay(decimal amount) => $"Credit Card payment of R{amount:F2} approved!";
        public string GetName() => "Credit Card";
    }

    public class EFTPayment : IPaymentStrategy
    {
        public string Pay(decimal amount) => $"EFT payment of R{amount:F2} initiated!";
        public string GetName() => "EFT";
    }

    public class PayFastPayment : IPaymentStrategy
    {
        public string Pay(decimal amount) => $"PayFast payment of R{amount:F2} successful!";
        public string GetName() => "PayFast";
    }

    public static class PaymentFactory
    {
        public static IPaymentStrategy Create(string method)
        {
            return method?.ToLower() switch
            {
                "credit card" => new CreditCardPayment(),
                "eft" => new EFTPayment(),
                "payfast" => new PayFastPayment(),
                _ => new CreditCardPayment()
            };
        }
    }
}