

namespace OneView.Models
{
    public class Sensordata
    {
        private DateTime _timeUtc = DateTime.UtcNow;
        private float _speedKmh;
        private float _inclineAngleDegLeft;
        private float _inclineAngleDegRight;
        private float _batteryPercent;


        public DateTime TimeUtc
        {
            get { return _timeUtc; }
            private set { _timeUtc = value; }
        }

        public float SpeedKmh
        {
            get { return _speedKmh; }
            private set { _speedKmh = value; }
        }

        public float InclineAngleDegLeft
        {
            get { return _inclineAngleDegLeft; }
            private set { _inclineAngleDegLeft = value; }
        }
        public float InclineAngleDegRight
        {
            get { return _inclineAngleDegRight; }
            private set { _inclineAngleDegRight = value; }
        }
        public float BatteryPercent
        {
            get { return _batteryPercent; }
            private set { _batteryPercent = value; }
        }
        // This All Methods will be started on App start and stop on App close
        public void UpdateBattery(float batteryLevel)
        {
            BatteryPercent = batteryLevel;

        }
        public void UpdateInclineAngle(float left, float right)
        {
            InclineAngleDegLeft = left;
            InclineAngleDegRight = right;
        }

        public void UpdateGps(float speed)
        {
            SpeedKmh = speed;

        }

    }

}
