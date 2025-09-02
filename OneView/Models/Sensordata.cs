using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneView.Models
{
    public class Sensordata
    {
        private DateTime _timeUtc = DateTime.UtcNow;
        private double _lat, _lon, _speedKmh, _headingDeg;
        private double _inclineAngleDeg, _pitchDeg;
        private int _batteryPercent;


    }
}
