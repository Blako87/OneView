using Microsoft.Maui.Devices.Sensors;
using System.Diagnostics;
using OneView.Models;

namespace OneView.Services
{
    /// <summary>
    /// Manages all device hardware sensors (Battery, Accelerometer, GPS)
    /// Continuously reads sensor data and updates CurrentSensorData
    /// Provides start/stop methods for each sensor with proper permission handling
    /// All sensor data is accessible via the public CurrentSensorData property
    /// </summary>
    public class SensorService
    {
        public event EventHandler<Sensordata>? SensorDataUpdated;
        // Live sensor data accessible to the entire app
        // Updated automatically as sensor events fire
        public Sensordata CurrentSensorData { get; private set; } = new();

        // Tracking flags for each sensor (prevents duplicate subscriptions)
        private bool _isBatteryWatched;
        private bool _isAccelerometerWatched = false;
        private bool _isGpsWatched = false;

        // Accelerometer state variables for calculating tilt angles
        private float lastRoll;        // Previous roll angle for delta calculation
        private float leftIncline;     // Current left tilt angle
        private float rightIncline;    // Current right tilt angle
        private void NotifySensorDataUpdated()
        {
            SensorDataUpdated?.Invoke(this, CurrentSensorData);
        }                           

        #region Battery Monitoring

        /// <summary>
        /// Starts monitoring device battery level
        /// Subscribes to Battery.Default.BatteryInfoChanged event
        /// Updates CurrentSensorData.BatteryPercent when battery level changes
        /// Safe to call multiple times (checks if already watching)
        /// </summary>
        public void StartWatchingBattery()
        {
            // Prevent duplicate event subscriptions
            if (!_isBatteryWatched)
            {
                // Subscribe to battery change events
                Battery.Default.BatteryInfoChanged += Battery_BatteryInfoChanged;
                _isBatteryWatched = true;
                Debug.WriteLine("✅ Battery monitoring started");
            }
        }

        /// <summary>
        /// Stops monitoring device battery level
        /// Unsubscribes from battery change events to prevent memory leaks
        /// </summary>
        public void StopWatchingBattery()
        {
            if (_isBatteryWatched)
            {
                // Unsubscribe from battery events
                Battery.Default.BatteryInfoChanged -= Battery_BatteryInfoChanged;
                _isBatteryWatched = false;
                Debug.WriteLine("🛑 Battery monitoring stopped");
            }
        }

        /// <summary>
        /// Event handler called when battery level changes
        /// Converts charge level (0.0-1.0) to percentage (0-100)
        /// Updates CurrentSensorData with new battery level
        /// </summary>
        private void Battery_BatteryInfoChanged(object? sender, BatteryInfoChangedEventArgs e)
        {
            // Convert decimal charge level to percentage (0.85 → 85%)
            float currentLevel = (float)(e.ChargeLevel * 100);
            CurrentSensorData.UpdateBattery(currentLevel);
            NotifySensorDataUpdated();
        }

        #endregion

        #region Accelerometer Monitoring

        /// <summary>
        /// Starts monitoring device accelerometer for tilt/lean angles
        /// Used to track bike lean angle during turns
        /// Calculates roll angle from X, Y, Z acceleration values
        /// Updates happen at SensorSpeed.UI frequency (varies by platform)
        /// Safe to call multiple times (checks if already watching)
        /// </summary>
        public void StartAccelerometer()
        {
            if (!_isAccelerometerWatched)
            {
                // Check if accelerometer is available on this device
                if (Accelerometer.Default.IsSupported)
                {
                    // Subscribe to accelerometer change events
                    Accelerometer.Default.ReadingChanged += Accelerometer_ReadingChanged;
                    // Start reading at UI-appropriate frequency
                    Accelerometer.Default.Start(SensorSpeed.UI);
                    _isAccelerometerWatched = true;
                    Debug.WriteLine("✅ Accelerometer started");
                }
                else
                {
                    Debug.WriteLine("⚠️ Accelerometer not supported on this device");
                }
            }
        }

        /// <summary>
        /// Stops monitoring accelerometer
        /// Unsubscribes from events and stops hardware sensor
        /// </summary>
        public void StopAccelerometer()
        {
            if (_isAccelerometerWatched)
            {
                Accelerometer.Default.Stop();
                Accelerometer.Default.ReadingChanged -= Accelerometer_ReadingChanged;
                _isAccelerometerWatched = false;
                Debug.WriteLine("🛑 Accelerometer stopped");
            }
        }

        /// <summary>
        /// Event handler called when accelerometer reading changes
        /// Calculates bike tilt angle (roll) from acceleration vectors
        /// Determines if bike is leaning left (negative roll) or right (positive roll)
        /// Updates CurrentSensorData with left/right incline angles
        /// </summary>
        /// <param name="sender">Event sender (unused)</param>
        /// <param name="e">Accelerometer data containing X, Y, Z acceleration</param>
        private void Accelerometer_ReadingChanged(object? sender, AccelerometerChangedEventArgs e)
        {
            var data = e.Reading;
            
            // Get acceleration components (in G's, where 1G = 9.8 m/s²)
            float x = data.Acceleration.X;
            float y = data.Acceleration.Y;
            float z = data.Acceleration.Z;
            
            // Calculate roll angle using arctangent of Y/Z acceleration
            // Roll = atan2(Y, Z) converted from radians to degrees
            // This represents rotation around the phone's length axis (bike lean)
            float roll = (float)(Math.Atan2(y, z) * 180 / Math.PI);
            float delta = roll;

            // Determine lean direction based on roll sign
            if (delta > 0)
            {
                // Positive roll = leaning right
                rightIncline = delta;
                leftIncline = 0;
            }
            else
            {
                // Negative roll = leaning left (convert to positive angle)
                leftIncline = Math.Abs(delta);
                rightIncline = 0;
            }

            // Store current roll for future delta calculations
            lastRoll = roll;
            
            // Update sensor data with new incline angles
            CurrentSensorData.UpdateInclineAngle(leftIncline, rightIncline);
            NotifySensorDataUpdated();
        }

        #endregion

        #region GPS Monitoring

        /// <summary>
        /// Starts monitoring GPS location for speed tracking
        /// Requests location permissions if not already granted
        /// Uses GeolocationAccuracy.Best for most accurate GPS fix
        /// Updates CurrentSensorData.SpeedKmh when location changes
        /// Safe to call multiple times (checks if already watching)
        /// </summary>
        /// <returns>True if GPS started successfully, false on error or permission denial</returns>
        public async Task<bool> StartWatchingGps()
        {
            // Prevent duplicate GPS sessions
            if (_isGpsWatched)
            {
                Debug.WriteLine("⚠️ GPS already watching");
                return true;
            }

            try
            {
                // Check if we're not already listening for location updates
                if (!Geolocation.Default.IsListeningForeground)
                {
                    Debug.WriteLine("🔍 Checking GPS permissions...");

                    // Check location permission status
                    var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                    // Request permission if not granted
                    if (status != PermissionStatus.Granted)
                    {
                        Debug.WriteLine("⚠️ Location permission not granted, requesting...");
                        status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    }

                    // Cannot proceed without permission
                    if (status != PermissionStatus.Granted)
                    {
                        Debug.WriteLine("❌ Location permission denied - GPS cannot start");
                        return false;
                    }

                    Debug.WriteLine("✅ Location permission granted");

                    // Configure GPS request for best accuracy
                    var request = new GeolocationListeningRequest(GeolocationAccuracy.Best)
                    {
                        DesiredAccuracy = GeolocationAccuracy.Best
                    };

                    // Subscribe to location change events
                    Geolocation.Default.LocationChanged += OnLocationChanged;
                    
                    // Start listening for GPS updates
                    await Geolocation.Default.StartListeningForegroundAsync(request);

                    _isGpsWatched = true;
                    Debug.WriteLine("✅ GPS monitoring started");
                    return true;
                }
                else
                {
                    Debug.WriteLine("⚠️ GPS already listening");
                    return true;
                }
            }
            catch (FeatureNotSupportedException ex)
            {
                // Device doesn't have GPS hardware
                Debug.WriteLine($"❌ GPS not supported: {ex.Message}");
                return false;
            }
            catch (PermissionException ex)
            {
                // Permission request failed
                Debug.WriteLine($"❌ GPS permission error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                // Other errors (GPS disabled, etc.)
                Debug.WriteLine($"❌ GPS error: {ex.Message}");
                Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Stops monitoring GPS location
        /// Unsubscribes from events and stops GPS hardware to save battery
        /// </summary>
        public void StopWatchingGps()
        {
            if (_isGpsWatched)
            {
                try
                {
                    // Unsubscribe from location events
                    Geolocation.Default.LocationChanged -= OnLocationChanged;
                    
                    // Stop GPS hardware
                    Geolocation.Default.StopListeningForeground();
                    
                    _isGpsWatched = false;
                    Debug.WriteLine("🛑 GPS monitoring stopped");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ Error stopping GPS: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Event handler called when GPS location changes
        /// Extracts speed from location data and converts from m/s to km/h
        /// Updates CurrentSensorData with new speed value
        /// Handles invalid or missing speed data gracefully
        /// </summary>
        /// <param name="sender">Event sender (unused)</param>
        /// <param name="e">Location data containing coordinates, accuracy, and speed</param>
        private void OnLocationChanged(object? sender, GeolocationLocationChangedEventArgs e)
        {
            try
            {
                if (e.Location != null)
                {
                    // Check if speed data is available and valid
                    if (e.Location.Speed.HasValue && e.Location.Speed.Value >= 0)
                    {
                        // Convert speed from meters/second to kilometers/hour
                        // Formula: km/h = m/s × 3.6
                        float speedKmh = (float)(e.Location.Speed.Value * 3.6);
                        CurrentSensorData.UpdateGps(speedKmh);
                        NotifySensorDataUpdated();
                        
                        Debug.WriteLine($"📍 GPS: Speed={speedKmh:F1} km/h, Accuracy={e.Location.Accuracy:F1}m");
                    }
                    else
                    {
                        // Speed not available or invalid, set to 0
                        CurrentSensorData.UpdateGps(0);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error processing GPS data: {ex.Message}");
            }
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Cleans up all sensor subscriptions and stops monitoring
        /// Should be called when the service is no longer needed
        /// Prevents memory leaks by unsubscribing from all events
        /// </summary>
        public void Dispose()
        {
            StopWatchingBattery();
            StopAccelerometer();
            StopWatchingGps();
            Debug.WriteLine("🧹 SensorService disposed");
        }

        #endregion
    }
}
