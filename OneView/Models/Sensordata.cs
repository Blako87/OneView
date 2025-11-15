namespace OneView.Models
{
    /// <summary>
    /// Represents real-time sensor data from device hardware (GPS, Battery, Accelerometer)
    /// This class holds live sensor readings that are continuously updated by SensorService
    /// </summary>
    public class Sensordata
    {
        // Private backing fields for encapsulation
        private DateTime _timeUtc = DateTime.UtcNow;
        private float _speedKmh;
        private float _inclineAngleDegLeft;
        private float _inclineAngleDegRight;
        private float _batteryPercent;

        /// <summary>
        /// UTC timestamp of the last sensor data update
        /// Updated whenever any sensor value changes
        /// </summary>
        public DateTime TimeUtc
        {
            get { return _timeUtc; }
            set { _timeUtc = value; }
        }

        /// <summary>
        /// Current GPS speed in kilometers per hour
        /// Calculated from GPS location updates (m/s converted to km/h)
        /// Private setter ensures only update methods can modify this value
        /// </summary>
        public float SpeedKmh
        {
            get { return _speedKmh; }
            private set { _speedKmh = value; }
        }

        /// <summary>
        /// Current left tilt angle in degrees (from accelerometer)
        /// Positive values indicate tilt to the left
        /// Used for tracking bike lean angle during turns
        /// </summary>
        public float InclineAngleDegLeft
        {
            get { return _inclineAngleDegLeft; }
            private set { _inclineAngleDegLeft = value; }
        }

        /// <summary>
        /// Current right tilt angle in degrees (from accelerometer)
        /// Positive values indicate tilt to the right
        /// Used for tracking bike lean angle during turns
        /// </summary>
        public float InclineAngleDegRight
        {
            get { return _inclineAngleDegRight; }
            private set { _inclineAngleDegRight = value; }
        }

        /// <summary>
        /// Current device battery level as a percentage (0-100)
        /// Updated from Battery.Default.BatteryInfoChanged events
        /// </summary>
        public float BatteryPercent
        {
            get { return _batteryPercent; }
            private set { _batteryPercent = value; }
        }

        /// <summary>
        /// Updates the battery percentage and timestamp
        /// Called by SensorService when battery level changes
        /// </summary>
        /// <param name="batteryLevel">Battery percentage (0-100)</param>
        public void UpdateBattery(float batteryLevel)
        {
            BatteryPercent = batteryLevel;
            TimeUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates both left and right incline angles and timestamp
        /// Called by SensorService when accelerometer readings change
        /// </summary>
        /// <param name="left">Left tilt angle in degrees</param>
        /// <param name="right">Right tilt angle in degrees</param>
        public void UpdateInclineAngle(float left, float right)
        {
            InclineAngleDegLeft = left;
            InclineAngleDegRight = right;
            TimeUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates GPS speed and timestamp
        /// Called by SensorService when location changes and speed is available
        /// </summary>
        /// <param name="speed">Speed in kilometers per hour</param>
        public void UpdateGps(float speed)
        {
            SpeedKmh = speed;
            TimeUtc = DateTime.UtcNow;
        }
    }
}
