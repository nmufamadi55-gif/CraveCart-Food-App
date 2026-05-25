using System;
using System.Collections.Generic;
using System.Linq;

namespace CraveCart.Services
{
    public class DriverTrackingService
    {
        private static readonly string[] DriverNames = { "Mike", "Sarah", "John", "Lisa", "David", "Anna", "James", "Maria", "Kevin", "Jessica" };
        private static readonly string[] Locations = { "Restaurant", "En Route", "Near Customer", "Arriving Soon", "Delivered" };

        private Random _random = new Random();
        private string _currentDriver;
        private string _currentStatus;
        private int _etaMinutes;
        private double _latitude;
        private double _longitude;
        private List<LocationHistory> _history;
        private DateTime _startTime;

        public DriverTrackingService()
        {
            _history = new List<LocationHistory>();
            ResetDriver();
        }

        public void ResetDriver()
        {
            _currentDriver = DriverNames[_random.Next(DriverNames.Length)];
            _currentStatus = "Available";
            _etaMinutes = 0;
            _latitude = -26.1952;  // Johannesburg coordinates
            _longitude = 28.0341;
            _history.Clear();
        }

        public void AssignDriver(string restaurantName, string customerAddress)
        {
            _currentStatus = "Assigned";
            _etaMinutes = _random.Next(20, 45);
            _startTime = DateTime.Now;
            _history.Add(new LocationHistory
            {
                Timestamp = DateTime.Now,
                Status = "Driver Assigned",
                Location = "Restaurant Hub",
                Eta = _etaMinutes
            });
        }

        public DriverInfo UpdateTracking()
        {
            if (_currentStatus == "Assigned" || _currentStatus == "En Route")
            {
                // Simulate driver movement
                var elapsed = (DateTime.Now - _startTime).TotalMinutes;
                var progress = Math.Min(1.0, elapsed / _etaMinutes);

                if (progress >= 1.0)
                {
                    _currentStatus = "Arrived";
                    _etaMinutes = 0;
                    _history.Add(new LocationHistory
                    {
                        Timestamp = DateTime.Now,
                        Status = "Arrived at Customer",
                        Location = "Customer Address",
                        Eta = 0
                    });
                }
                else if (progress > 0.7)
                {
                    _currentStatus = "Near Customer";
                    _etaMinutes = (int)(_etaMinutes * (1 - progress));
                    if (_history.Last().Status != "Near Customer")
                        _history.Add(new LocationHistory
                        {
                            Timestamp = DateTime.Now,
                            Status = "Near Customer",
                            Location = "Near Destination",
                            Eta = _etaMinutes
                        });
                }
                else if (progress > 0.3)
                {
                    _currentStatus = "En Route";
                    _etaMinutes = (int)(_etaMinutes * (1 - progress));
                    if (_history.Last().Status != "En Route")
                        _history.Add(new LocationHistory
                        {
                            Timestamp = DateTime.Now,
                            Status = "En Route",
                            Location = "On The Way",
                            Eta = _etaMinutes
                        });
                }

                // Update mock GPS coordinates
                _latitude += (_random.NextDouble() - 0.5) * 0.01;
                _longitude += (_random.NextDouble() - 0.5) * 0.01;
            }

            return new DriverInfo
            {
                DriverName = _currentDriver,
                Status = _currentStatus,
                EtaMinutes = _etaMinutes,
                Latitude = _latitude,
                Longitude = _longitude,
                History = _history.ToList(),
                StartTime = _startTime
            };
        }

        public void CompleteDelivery()
        {
            _currentStatus = "Completed";
            _history.Add(new LocationHistory
            {
                Timestamp = DateTime.Now,
                Status = "Delivered",
                Location = "Customer Address",
                Eta = 0
            });
        }

        public DriverInfo GetCurrentStatus()
        {
            return new DriverInfo
            {
                DriverName = _currentDriver,
                Status = _currentStatus,
                EtaMinutes = _etaMinutes,
                Latitude = _latitude,
                Longitude = _longitude,
                History = _history.ToList(),
                StartTime = _startTime
            };
        }
    }

    public class DriverInfo
    {
        public string DriverName { get; set; }
        public string Status { get; set; }
        public int EtaMinutes { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<LocationHistory> History { get; set; }
        public DateTime StartTime { get; set; }
        public string VehicleType { get; internal set; }
        public string Email { get; internal set; }
        public string Phone { get; internal set; }
        public double Rating { get; internal set; }
        public int TotalDeliveries { get; internal set; }
    }

    public class LocationHistory
    {
        public DateTime Timestamp { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }
        public int Eta { get; set; }
    }
}