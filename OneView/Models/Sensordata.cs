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
        private double _speedKmh;
        private double _inclineAngleDegLeft, _inclineAngleDegRight;
        private double _batteryPercent;
        public Rideprofile _rideprofile = new Rideprofile();

        public DateTime TimeUtc
        {
            get { return _timeUtc; }
            private set { _timeUtc = value; }
        }

        public double SpeedKmh
        {
            get { return _speedKmh; }
            private set { _speedKmh = value; }
        }

        public double InclineAngleDeg
        {
            get { return _inclineAngleDegLeft; }
            private set { _inclineAngleDegLeft = value; }
        }
        public double PitchDeg
        {
            get { return _inclineAngleDegRight; }
            private set { _inclineAngleDegRight = value; }
        }
        public double BatteryPercent
        {
            get { return _batteryPercent; }
            private set { _batteryPercent = value; }
        }
        // This All Methods will be started on App start and stop on App close
        public void UpdateBattery(double batteryLevel)
        {
            _batteryPercent = batteryLevel;

        }
        public void UpdateInclineAngle(double left, double right)
        {
            _inclineAngleDegLeft = left;
            _inclineAngleDegRight = right;
        }
       
        public void UpdateGps(double speed)
        {
            _speedKmh = speed;
            _rideprofile.AktuallSpeed(_speedKmh);

        }

    }

}
