

using OneView.Models;


namespace OneView.Services
{
    public class SensorService
    {
        public Sensordata _currentSensorData = new Sensordata();

        private bool _isBatteryWatched = false;

        public void StartWatchingBattery()
        {

            if (!_isBatteryWatched)
            {

                Battery.Default.BatteryInfoChanged += Battery_BatteryInfoChanged;
                _isBatteryWatched = true;
            }

        }
        public void StopWatchingBattery()
        {
            if (_isBatteryWatched)
            {
                Battery.Default.BatteryInfoChanged -= Battery_BatteryInfoChanged;
                
                _isBatteryWatched = false;
            }
        }

        private void Battery_BatteryInfoChanged(object? sender, BatteryInfoChangedEventArgs e)
        {
            double currentLevel = e.ChargeLevel * 100;
            _currentSensorData.UpdateBattery(currentLevel);
        }

        private bool _isAccelerometerWatched = false;
        double lastRoll;
        double leftIncline;
        double rightIncline;

        public void StartAccelerometer()
        {
            if (!_isAccelerometerWatched)
            {

                if (Accelerometer.Default.IsSupported)
                {
                    Accelerometer.Default.ReadingChanged += Accelerometer_ReadingChanged;
                    Accelerometer.Default.Start(SensorSpeed.UI);
                   
                    _isAccelerometerWatched = true;
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
            }

        }
        private void Accelerometer_ReadingChanged(object? sender, AccelerometerChangedEventArgs e)
        {
            var data = e.Reading;
            double x = data.Acceleration.X;
            double y = data.Acceleration.Y;
            double z = data.Acceleration.Z;
            double roll = Math.Atan2(y, z) * 180 / Math.PI;
            double delta = roll - lastRoll;
            if (delta > 0)
            {
                rightIncline += delta;
            }
            else if (delta < 0)
            {
                leftIncline -= delta;
            }
            lastRoll = roll;
            _currentSensorData.UpdateInclineAngle(leftIncline, rightIncline);

        }

        public async Task StartWatchingGps()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status == PermissionStatus.Granted)
            {
                Geolocation.Default.LocationChanged += OnLocationChanged;
                await Geolocation.Default.StartListeningForegroundAsync(new GeolocationListeningRequest(GeolocationAccuracy.Medium));
            }
        }
        public void StopWatchingGps()
        {
            Geolocation.Default.LocationChanged -= OnLocationChanged;
            Geolocation.Default.StopListeningForeground();
        }

        private void OnLocationChanged(object? sender, GeolocationLocationChangedEventArgs e)
        {
            if (e.Location.Speed.HasValue)
            {
                double speedKmh = e.Location.Speed.Value * 3.6; // Convert m/s to km/h
                _currentSensorData.UpdateGps(speedKmh);
            }
        }

        public void Dispose()
        {
            StopWatchingBattery();
            StopAccelerometer();
        }

    }
}
