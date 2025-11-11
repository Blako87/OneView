using Microsoft.Maui.Devices.Sensors;
using System.Diagnostics;
using OneView.Models;

namespace OneView.Services
{
    public class SensorService
    {
        public Sensordata CurrentSensorData { get; private set; } = new();

        private bool _isBatteryWatched;
        private bool _isAccelerometerWatched = false;
        private bool _isGpsWatched = false;

        private float lastRoll;
        private float leftIncline;
        private float rightIncline;

        // Battery Monitoring
        public void StartWatchingBattery()
        {
            if (!_isBatteryWatched)
            {
                Battery.Default.BatteryInfoChanged += Battery_BatteryInfoChanged;
                _isBatteryWatched = true;
                Debug.WriteLine("✅ Battery monitoring started");
            }
        }

        public void StopWatchingBattery()
        {
            if (_isBatteryWatched)
            {
                Battery.Default.BatteryInfoChanged -= Battery_BatteryInfoChanged;
                _isBatteryWatched = false;
                Debug.WriteLine("🛑 Battery monitoring stopped");
            }
        }

        private void Battery_BatteryInfoChanged(object? sender, BatteryInfoChangedEventArgs e)
        {
            float currentLevel = (float)(e.ChargeLevel * 100);
            CurrentSensorData.UpdateBattery(currentLevel);
        }

        // Accelerometer Monitoring
        public void StartAccelerometer()
        {
            if (!_isAccelerometerWatched)
            {
                if (Accelerometer.Default.IsSupported)
                {
                    Accelerometer.Default.ReadingChanged += Accelerometer_ReadingChanged;
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

        private void Accelerometer_ReadingChanged(object? sender, AccelerometerChangedEventArgs e)
        {
            var data = e.Reading;
            float x = data.Acceleration.X;
            float y = data.Acceleration.Y;
            float z = data.Acceleration.Z;
            float roll = (float)(Math.Atan2(y, z) * 180 / Math.PI);
            float delta = roll;

            if (delta > 0)
            {
                rightIncline = delta;
                leftIncline = 0;
            }
            else
            {
                leftIncline = Math.Abs(delta);
                rightIncline = 0;
            }

            lastRoll = roll;
            CurrentSensorData.UpdateInclineAngle(leftIncline, rightIncline);
        }

        // GPS Monitoring
        public async Task<bool> StartWatchingGps()
        {
            if (_isGpsWatched)
            {
                Debug.WriteLine("⚠️ GPS already watching");
                return true;
            }

            try
            {
                // Check if location is supported
                if (!Geolocation.Default.IsListeningForeground)
                {
                    Debug.WriteLine("🔍 Checking GPS permissions...");

                    // Check location permission
                    var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                    if (status != PermissionStatus.Granted)
                    {
                        Debug.WriteLine("⚠️ Location permission not granted, requesting...");
                        status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    }

                    if (status != PermissionStatus.Granted)
                    {
                        Debug.WriteLine("❌ Location permission denied - GPS cannot start");
                        return false;
                    }

                    Debug.WriteLine("✅ Location permission granted");

                    // Start listening to location changes
                    var request = new GeolocationListeningRequest(GeolocationAccuracy.Best)
                    {
                        DesiredAccuracy = GeolocationAccuracy.Best
                    };

                    Geolocation.Default.LocationChanged += OnLocationChanged;
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
                Debug.WriteLine($"❌ GPS not supported: {ex.Message}");
                return false;
            }
            catch (PermissionException ex)
            {
                Debug.WriteLine($"❌ GPS permission error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ GPS error: {ex.Message}");
                Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public void StopWatchingGps()
        {
            if (_isGpsWatched)
            {
                try
                {
                    Geolocation.Default.LocationChanged -= OnLocationChanged;
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

        private void OnLocationChanged(object? sender, GeolocationLocationChangedEventArgs e)
        {
            try
            {
                if (e.Location != null)
                {
                    if (e.Location.Speed.HasValue && e.Location.Speed.Value >= 0)
                    {
                        float speedKmh = (float)(e.Location.Speed.Value * 3.6); // Convert m/s to km/h
                        CurrentSensorData.UpdateGps(speedKmh);
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

        // Cleanup
        public void Dispose()
        {
            StopWatchingBattery();
            StopAccelerometer();
            StopWatchingGps();
            Debug.WriteLine("🧹 SensorService disposed");
        }
    }
}
