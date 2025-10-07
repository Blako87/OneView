using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneView.Models
{
    public class Displaydata
    {
        public double SpeedKmh { get; set; }
        public double InclineAngleDeg { get; set; }
        public double BatteryPercent { get; set; }
        public double TripDistanceKm { get; set; }
        public TimeOnly TimeOnBike { get; set; }

        public void BatteryPercentage(double batteryLevel)
        {
            BatteryPercent = batteryLevel;
        }
    }
}
