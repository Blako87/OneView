using System.Diagnostics;
using OneView.Models;

namespace OneView.Services
{
    /// <summary>
    /// Manages bike ride tracking and statistics calculation
    /// Reads live data from SensorService and calculates ride metrics (distance, speed, duration, angles)
    /// Automatically saves ride data to disk when ride ends using SaveprofileData
    /// Updates statistics every 60 seconds during an active ride
    /// </summary>
    public class ProfileService
    {
        // Current ride profile being tracked
        public Rideprofile CurrentRideProfile { get; private set; } = new();
        
        // Persistence service for saving/loading ride data
        private readonly SaveprofileData _saveService = new();

        // Timer for periodic statistics updates (now every 1 second)
        private System.Timers.Timer? _updateTimer;
        
        // Flag indicating if a ride is currently active
        private bool _isRideActive = false;
        
        // Timestamp when the current ride started
        private DateTime _rideStartTime;

        // Timestamp of the last statistics update
        private DateTime _lastUpdateTime;

        // Update interval in milliseconds (1 second for smoother distance updates)
        private const int UpdateIntervalMs = 1000;

        /// <summary>
        /// Starts tracking a new bike ride
        /// Resets all statistics to zero and begins periodic updates every second
        /// Cannot start a new ride if one is already active
        /// </summary>
        public void StartRide()
        {
            // Prevent starting multiple rides simultaneously
            if (_isRideActive)
            {
                Debug.WriteLine("⚠️ Ride already active");
                return;
            }

            // Mark ride as active
            _isRideActive = true;
            _rideStartTime = DateTime.Now;
            _lastUpdateTime = _rideStartTime;
            CurrentRideProfile.LastTime = _rideStartTime;

            // Reset all ride statistics to starting values
            CurrentRideProfile.Distance = 0;
            CurrentRideProfile.MaxSpeed = 0;
            CurrentRideProfile.TimeOnBike = TimeSpan.Zero;
            CurrentRideProfile.MinInclineAngleLeft = 0;
            CurrentRideProfile.MaxInclineAngleLeft = 0;
            CurrentRideProfile.MinInclineAngleRight = 0;
            CurrentRideProfile.MaxInclineAngleRight = 0;

            // Create and start timer for periodic statistics updates (every 1,000 ms)
            _updateTimer = new System.Timers.Timer(UpdateIntervalMs);
            _updateTimer.Elapsed += UpdateRideStatistics;
            _updateTimer.AutoReset = true; // Keep firing
            _updateTimer.Start();

            Debug.WriteLine($"✅ Ride started at {_rideStartTime:HH:mm:ss}");
        }

        /// <summary>
        /// Stops the current ride and saves statistics to disk
        /// Performs final statistics update before saving
        /// Cannot stop if no ride is active
        /// </summary>
        /// <returns>True if ride was stopped and data saved successfully, false on error or no active ride</returns>
        public bool StopRide()
        {
            // Check if there's an active ride to stop
            if (!_isRideActive)
            {
                Debug.WriteLine("⚠️ No active ride to stop");
                return false;
            }

            // Do a final stats update using the exact elapsed time since last tick
            FinalizeRideStatistics();

            // Mark ride as inactive
            _isRideActive = false;

            // Stop and dispose of the update timer
            if (_updateTimer != null)
            {
                _updateTimer.Stop();
                _updateTimer.Elapsed -= UpdateRideStatistics;
                _updateTimer.Dispose();
                _updateTimer = null;
            }

            // Perform final calculations
            UpdateTimeOnBike();
            UpdateMediumSpeed();

            Debug.WriteLine($"🛑 Ride stopped. Duration: {CurrentRideProfile.TimeOnBike}, Distance: {CurrentRideProfile.Distance:F2} km");

            // Save ride data to JSON file
            bool saved = _saveService.SaveRideData(CurrentRideProfile);
            if (saved)
            {
                Debug.WriteLine("✅ Ride data saved successfully");
            }
            else
            {
                Debug.WriteLine("❌ Failed to save ride data");
            }

            return saved;
        }

        /// <summary>
        /// Periodic callback that updates ride statistics on timer interval
        /// Uses elapsed time since last update for precise distance integration
        /// </summary>
        private void UpdateRideStatistics(object? sender, System.Timers.ElapsedEventArgs e)
        {
            // Safety check - only update if ride is active
            if (!_isRideActive)
                return;

            try
            {
                // Get current live sensor data from the global SensorService
                var sensorData = App.SensorService.CurrentSensorData;

                // Compute elapsed time since last update
                var now = DateTime.Now;
                var delta = now - _lastUpdateTime;
                if (delta <= TimeSpan.Zero)
                {
                    // Guard against clock issues
                    delta = TimeSpan.FromMilliseconds(UpdateIntervalMs);
                }

                // Update total time on bike and last time
                UpdateTimeOnBike();

                // Calculate distance for this interval
                double speedKmh = sensorData.SpeedKmh;
                double intervalHours = delta.TotalHours; // precise elapsed time
                double distanceThisInterval = speedKmh * intervalHours; // kilometers
                CurrentRideProfile.Distance += distanceThisInterval;

                // Update last update timestamp
                _lastUpdateTime = now;

                // Update maximum speed if current speed is higher
                if (speedKmh > CurrentRideProfile.MaxSpeed)
                {
                    CurrentRideProfile.MaxSpeed = speedKmh;
                }

                // Update current instantaneous speed
                CurrentRideProfile.Speed = speedKmh;

                // Update incline angle min/max values
                UpdateInclineAngles(sensorData.InclineAngleDegLeft, sensorData.InclineAngleDegRight);

                // Recalculate average speed
                UpdateMediumSpeed();

                Debug.WriteLine($"📊 Ride Update: +{distanceThisInterval:F3}km, Total={CurrentRideProfile.Distance:F2}km, Speed={speedKmh:F1}km/h, Max={CurrentRideProfile.MaxSpeed:F1}km/h");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error updating ride statistics: {ex.Message}");
            }
        }

        /// <summary>
        /// Ensures a last distance integration occurs at ride stop
        /// </summary>
        private void FinalizeRideStatistics()
        {
            try
            {
                var sensorData = App.SensorService.CurrentSensorData;
                var now = DateTime.Now;
                var delta = now - _lastUpdateTime;
                if (delta > TimeSpan.Zero)
                {
                    double intervalHours = delta.TotalHours;
                    double distanceThisInterval = sensorData.SpeedKmh * intervalHours;
                    CurrentRideProfile.Distance += distanceThisInterval;
                    _lastUpdateTime = now;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error finalizing ride statistics: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the total time on bike by calculating elapsed time since ride start
        /// Also updates the LastTime timestamp
        /// </summary>
        private void UpdateTimeOnBike()
        {
            DateTime now = DateTime.Now;
            // Calculate duration as difference between now and ride start time
            CurrentRideProfile.TimeOnBike = now - _rideStartTime;
            CurrentRideProfile.LastTime = now;
        }

        /// <summary>
        /// Calculates and updates the average (medium) speed for the entire ride
        /// Formula: average_speed = total_distance / total_time
        /// Returns 0 if ride duration is 0 (prevents division by zero)
        /// </summary>
        private void UpdateMediumSpeed()
        {
            // Convert TimeSpan to hours for calculation
            double timeHours = CurrentRideProfile.TimeOnBike.TotalHours;

            if (timeHours > 0)
            {
                // Average speed = total distance / total time
                CurrentRideProfile.MediumSpeed = CurrentRideProfile.Distance / timeHours;
            }
            else
            {
                CurrentRideProfile.MediumSpeed = 0;
            }
        }

        /// <summary>
        /// Updates the min/max incline angles (left and right)
        /// Tracks the extreme tilt values encountered during the ride
        /// </summary>
        /// <param name="leftAngle">Current left tilt angle in degrees</param>
        /// <param name="rightAngle">Current right tilt angle in degrees</param>
        private void UpdateInclineAngles(float leftAngle, float rightAngle)
        {
            // Track minimum and maximum LEFT tilt angles
            if (CurrentRideProfile.MinInclineAngleLeft == 0 || leftAngle < CurrentRideProfile.MinInclineAngleLeft)
            {
                CurrentRideProfile.MinInclineAngleLeft = leftAngle;
            }
            if (leftAngle > CurrentRideProfile.MaxInclineAngleLeft)
            {
                CurrentRideProfile.MaxInclineAngleLeft = leftAngle;
            }

            // Track minimum and maximum RIGHT tilt angles
            if (CurrentRideProfile.MinInclineAngleRight == 0 || rightAngle < CurrentRideProfile.MinInclineAngleRight)
            {
                CurrentRideProfile.MinInclineAngleRight = rightAngle;
            }
            if (rightAngle > CurrentRideProfile.MaxInclineAngleRight)
            {
                CurrentRideProfile.MaxInclineAngleRight = rightAngle;
            }
        }

        /// <summary>
        /// Loads previously saved ride data from disk
        /// Replaces current ride profile with loaded data
        /// Useful for viewing ride history
        /// </summary>
        /// <returns>Loaded Rideprofile object, or null if load failed</returns>
        public Rideprofile? LoadRideData()
        {
            var loadedProfile = _saveService.LoadRideData();
            if (loadedProfile != null)
            {
                CurrentRideProfile = loadedProfile;
                Debug.WriteLine($"✅ Loaded ride profile: Distance={CurrentRideProfile.Distance:F2}km, Time={CurrentRideProfile.TimeOnBike}");
            }
            return loadedProfile;
        }

        /// <summary>
        /// Gets whether a ride is currently active (being tracked)
        /// </summary>
        public bool IsRideActive => _isRideActive;

        /// <summary>
        /// Gets the ride duration as a formatted string (HH:mm:ss)
        /// Useful for displaying in UI
        /// </summary>
        /// <returns>Formatted duration string (e.g., "01:23:45")</returns>
        public string GetRideDurationFormatted()
        {
            return CurrentRideProfile.TimeOnBike.ToString(@"hh\:mm\:ss");
        }

        /// <summary>
        /// Clears all ride data and resets to a new empty profile
        /// Used when clearing all saved rides
        /// </summary>
        public void ClearRideData()
        {
            CurrentRideProfile = new Rideprofile();
            Debug.WriteLine("🗑️ Ride data cleared");
        }

        /// <summary>
        /// Cleans up resources (timer) when service is disposed
        /// Should be called when the service is no longer needed
        /// </summary>
        public void Dispose()
        {
            // Stop and dispose timer if it exists
            if (_updateTimer != null)
            {
                _updateTimer.Stop();
                _updateTimer.Elapsed -= UpdateRideStatistics;
                _updateTimer.Dispose();
                _updateTimer = null;
            }

            Debug.WriteLine("🧹 ProfileService disposed");
        }
    }
}

