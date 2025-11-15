using System.Text.Json.Serialization;

namespace OneView.Models
{
    /// <summary>
    /// Represents a complete bike ride profile with statistics and tracking data
    /// This model stores all ride-related metrics like distance, speed, duration, and incline angles
    /// Data is automatically saved to JSON when a ride ends
    /// </summary>
    public class Rideprofile
    {
        // Private backing fields
        private double _distance;
        private TimeSpan _timeOnBike = TimeSpan.Zero;
        private DateTime _lastTime = DateTime.Now;
        private double _mediumSpeed;
        private double _maxSpeed;
        private double _minInclineAngleLeft;
        private double _maxInclineAngleLeft;
        private double _minInclineAngleRight;
        private double _maxInclineAngleRight;
        private System.Timers.Timer? _ticks;
        private double _speed;

        /// <summary>
        /// Internal timer for periodic ride statistics updates
        /// Marked with [JsonIgnore] because System.Timers.Timer cannot be serialized to JSON
        /// Managed by ProfileService, not part of persistent data
        /// </summary>
        [JsonIgnore]
        public System.Timers.Timer? Ticks
        {
            get { return _ticks; }
            set { _ticks = value; }
        }

        /// <summary>
        /// Timestamp of the last ride update
        /// Used to calculate ride duration and track when statistics were last updated
        /// </summary>
        public DateTime LastTime
        {
            get { return _lastTime; }
            set { _lastTime = value; }
        }

        /// <summary>
        /// Current instantaneous speed in km/h
        /// Updated from sensor data during the ride
        /// </summary>
        public double Speed
        {
            get { return _speed; }
            set { _speed = value; }
        }

        /// <summary>
        /// Total distance traveled during the ride in kilometers
        /// Calculated by integrating speed over time: distance += speed × time_interval
        /// </summary>
        public double Distance
        {
            get { return _distance; }
            set { _distance = value; }
        }

        /// <summary>
        /// Total duration of the ride
        /// Calculated as current_time - ride_start_time
        /// </summary>
        public TimeSpan TimeOnBike
        {
            get { return _timeOnBike; }
            set { _timeOnBike = value; }
        }

        /// <summary>
        /// Average (medium) speed for the entire ride in km/h
        /// Calculated as: total_distance / total_time
        /// </summary>
        public double MediumSpeed
        {
            get { return _mediumSpeed; }
            set { _mediumSpeed = value; }
        }

        /// <summary>
        /// Maximum speed reached during the ride in km/h
        /// Tracks the highest speed value encountered
        /// </summary>
        public double MaxSpeed
        {
            get { return _maxSpeed; }
            set { _maxSpeed = value; }
        }

        /// <summary>
        /// Minimum left incline angle reached during the ride in degrees
        /// Tracks the most extreme left tilt (lowest value)
        /// </summary>
        public double MinInclineAngleLeft
        {
            get { return _minInclineAngleLeft; }
            set { _minInclineAngleLeft = value; }
        }

        /// <summary>
        /// Maximum left incline angle reached during the ride in degrees
        /// Tracks the most extreme left tilt (highest value)
        /// </summary>
        public double MaxInclineAngleLeft
        {
            get { return _maxInclineAngleLeft; }
            set { _maxInclineAngleLeft = value; }
        }

        /// <summary>
        /// Minimum right incline angle reached during the ride in degrees
        /// Tracks the most extreme right tilt (lowest value)
        /// </summary>
        public double MinInclineAngleRight
        {
            get { return _minInclineAngleRight; }
            set { _minInclineAngleRight = value; }
        }

        /// <summary>
        /// Maximum right incline angle reached during the ride in degrees
        /// Tracks the most extreme right tilt (highest value)
        /// </summary>
        public double MaxInclineAngleRight
        {
            get { return _maxInclineAngleRight; }
            set { _maxInclineAngleRight = value; }
        }
    }
}
