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

        public DateTime TimeUtc
        {
            get { return _timeUtc; }
            private set { _timeUtc = value; }
        }
        public double Lat
        {
            get { return _lat; }
            private set { _lat = value; }
        }
        public double Lon
        {
            get { return _lon; }
            private set { _lon = value; }

        }
        public double SpeedKmh
        {
            get { return _speedKmh;}
            private set { _speedKmh = value;}
        }
        public double HeadingDeg
        {
            get { return _headingDeg; }
            private set { _headingDeg = value; }
        }
        public double InclineAngleDeg
        {
            get { return _inclineAngleDeg; }
            private set { _inclineAngleDeg = value; }
        }
        public double PitchDeg
        {
            get { return _pitchDeg; }
            private set { _pitchDeg = value; }
        }
        public int BatteryPercent
        {
            get { return _batteryPercent; }
            private set { _batteryPercent = value; }
        }
    }
}
