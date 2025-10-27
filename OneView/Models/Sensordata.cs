

namespace OneView.Models
{
    public class Sensordata
    {
        private DateTime _timeUtc = DateTime.UtcNow;
        private double _speedKmh;
        private double _inclineAngleDegLeft;
        private double _inclineAngleDegRight;
        private double _batteryPercent;


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

        public double InclineAngleDegLeft
        {
            get { return _inclineAngleDegLeft; }
            private set { _inclineAngleDegLeft = value; }
        }
        public double InclineAngleDegRight
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

        }

    }

}
