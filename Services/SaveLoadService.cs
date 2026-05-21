using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CraveCart.Models;

namespace CraveCart.Services
{
    public class SaveLoadService
    {
        private readonly string _saveDirectory = "Data/Saves";

        public SaveLoadService()
        {
            if (!Directory.Exists(_saveDirectory))
                Directory.CreateDirectory(_saveDirectory);
        }

        public void SaveCart(List<CartItem> cart, int loyaltyPoints, string filename = "cart_save.json")
        {
            try
            {
                var saveData = new SaveData
                {
                    Items = cart,
                    LoyaltyPoints = loyaltyPoints,
                    SaveDate = DateTime.Now,
                    ItemCount = cart.Count,
                    TotalValue = cart.Sum(i => i.TotalPrice)
                };

                var json = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(Path.Combine(_saveDirectory, filename), json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Save error: {ex.Message}");
            }
        }

        public SaveData LoadCart(string filename = "cart_save.json")
        {
            try
            {
                var filePath = Path.Combine(_saveDirectory, filename);
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    return JsonSerializer.Deserialize<SaveData>(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Load error: {ex.Message}");
            }
            return null;
        }

        public List<string> GetSaveFiles()
        {
            try
            {
                return Directory.GetFiles(_saveDirectory, "*.json")
                    .Select(Path.GetFileName)
                    .ToList() ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        public void DeleteSave(string filename)
        {
            try
            {
                var filePath = Path.Combine(_saveDirectory, filename);
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch { }
        }
    }

    public class SaveData
    {
        public List<CartItem> Items { get; set; }
        public int LoyaltyPoints { get; set; }
        public DateTime SaveDate { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalValue { get; set; }
    }
}