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
            set { _timeUtc = value; }
        }

        public float SpeedKmh
        {
            get { return _speedKmh; }
            set { _speedKmh = value; }
        }

        public float InclineAngleDegLeft
        {
            get { return _inclineAngleDegLeft; }
            set { _inclineAngleDegLeft = value; }
        }

        public float InclineAngleDegRight
        {
            get { return _inclineAngleDegRight; }
            set { _inclineAngleDegRight = value; }
        }

        public float BatteryPercent
        {
            get { return _batteryPercent; }
            set { _batteryPercent = value; }
        }

        public void UpdateBattery(float batteryLevel)
        {
            BatteryPercent = batteryLevel;
            TimeUtc = DateTime.UtcNow;
        }

        public void UpdateInclineAngle(float left, float right)
        {
            InclineAngleDegLeft = left;
            InclineAngleDegRight = right;
            TimeUtc = DateTime.UtcNow;
        }

        public void UpdateGps(float speed)
        {
            SpeedKmh = speed;
            TimeUtc = DateTime.UtcNow;
        }
    }
}
