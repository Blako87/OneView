using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OneView.Models;
using System.Diagnostics;

namespace OneView.ViewModels
{
    /// <summary>
    /// ViewModel for the Main Dashboard page
    /// Displays live sensor data and Bluetooth status with auto-refresh
    /// </summary>
    public partial class MainViewModel : ObservableObject, IDisposable
    {
        private System.Timers.Timer? _refreshTimer;

        #region Sensor Data Properties

        [ObservableProperty]
        private float speed = 0;

        [ObservableProperty]
        private float batteryPercent = 0;

        [ObservableProperty]
        private float inclineLeft = 0;

        [ObservableProperty]
        private float inclineRight = 0;

        [ObservableProperty]
        private string timeOnBike = "00:00:00";

        [ObservableProperty]
        private double mediumSpeed = 0;

        [ObservableProperty]
        private double maxSpeed = 0;

        [ObservableProperty]
        private double minInclineLeft = 0;

        [ObservableProperty]
        private double maxInclineLeft = 0;

        [ObservableProperty]
        private double minInclineRight = 0;

        [ObservableProperty]
        private double maxInclineRight = 0;

        [ObservableProperty]
        private double distance = 0;

        #endregion

        #region Bluetooth Status Properties

        [ObservableProperty]
        private bool isBluetoothOn = false;

        [ObservableProperty]
        private bool isHelmetConnected = false;

        [ObservableProperty]
        private int packetsSent = 0;

        [ObservableProperty]
        private string lastTransmission = "Nie";

        #endregion

        #region UI State Properties

        [ObservableProperty]
        private string username = "Guest";

        [ObservableProperty]
        private bool isRideActive = false;

        [ObservableProperty]
        private string rideButtonText = "Fahrt Starten";

        [ObservableProperty]
        private string rideButtonIcon = "??";

        /// <summary>
        /// Indicates whether the user is logged in
        /// </summary>
        public bool IsLoggedIn => App.LoginService?.IsLoggedIn() ?? false;

        #endregion

        public MainViewModel()
        {
            // Load username
            Username = App.LoginService.GetUsername();
            App.SensorService.SensorDataUpdated += OnSensorDataUpdated;
            App.ProfileService.RideProfileUpdated += OnProfileDataUpdated;

            // Start auto-refresh timer (every 1000ms for smooth UI updates)
            _refreshTimer = new System.Timers.Timer(1000);
            _refreshTimer.Elapsed += (s, e) => RefreshNonEventData();
            _refreshTimer.AutoReset = true;
            _refreshTimer.Start();

            Debug.WriteLine("? MainViewModel initialized");
        }

        /// <summary>
        /// Refreshes all data from services
        /// Called automatically every 200ms
        /// </summary>
        private void OnSensorDataUpdated(object? sender, Sensordata sensorData)
        {
            try
            {
                // Get sensor data

                if (Math.Abs(speed - sensorData.SpeedKmh) > 0.1)
                {
                    Speed = sensorData.SpeedKmh;
                }

                if (Math.Abs(batteryPercent - sensorData.BatteryPercent) > 0.1)
                {
                    BatteryPercent = sensorData.BatteryPercent;
                }

                if (Math.Abs(inclineLeft - sensorData.InclineAngleDegLeft) > 0.1)
                {
                    InclineLeft = sensorData.InclineAngleDegLeft;
                }

                if (Math.Abs(inclineRight - sensorData.InclineAngleDegRight) > 0.1)
                {
                    InclineRight = sensorData.InclineAngleDegRight;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"? Error refreshing data: {ex.Message}");
            }
        }

        private void OnProfileDataUpdated(object? sender, Rideprofile profile)
        {
            // Get ride profile data
            try
            {
                if (Math.Abs(Distance - profile.Distance) > 0.1)
                {
                    Distance = profile.Distance;
                }

                if (Math.Abs(MediumSpeed - profile.MediumSpeed) > 0.1)
                {
                    MediumSpeed = profile.MediumSpeed;
                }

                if (Math.Abs(MaxSpeed - profile.MaxSpeed) > 0.1)
                {
                    MaxSpeed = profile.MaxSpeed;
                }

                if (Math.Abs(MinInclineLeft - profile.MinInclineAngleLeft) > 0.1)
                {
                    MinInclineLeft = profile.MinInclineAngleLeft;
                }

                if (Math.Abs(MaxInclineLeft - profile.MaxInclineAngleLeft) > 0.1)
                {
                    MaxInclineLeft = profile.MaxInclineAngleLeft;
                }

                if (Math.Abs(MinInclineRight - profile.MinInclineAngleRight) > 0.1)
                {
                    MinInclineRight = profile.MinInclineAngleRight;
                }

                if (Math.Abs(MaxInclineRight - profile.MaxInclineAngleRight) > 0.1)
                {
                    MaxInclineRight = profile.MaxInclineAngleRight;
                }

                TimeOnBike = App.ProfileService.GetRideDurationFormatted();
                // Update ride status
                IsRideActive = App.ProfileService.IsRideActive;
                UpdateRideButtonState();

            }

            catch (Exception ex)
            {
                Debug.WriteLine($"? Error refreshing data: {ex.Message}");
            }
        }
        private void RefreshNonEventData()
        {
            // Get Bluetooth status
            IsBluetoothOn = App.BluetoothService.IsBluetoothAvailable();
            IsHelmetConnected = App.BluetoothService.IsHelmetConnected();
            PacketsSent = App.BluetoothService.DataSentCount;

            var lastTime = App.BluetoothService.LastDataSentTime;
            LastTransmission = lastTime?.ToString("HH:mm:ss") ?? "Nie";
        }


        /// <summary>
        /// Updates the ride button text and icon based on ride state
        /// </summary>
        private void UpdateRideButtonState()
        {
            if (IsRideActive)
            {
                RideButtonText = "Fahrt Beenden";
                RideButtonIcon = "??";
            }
            else
            {
                RideButtonText = "Fahrt Starten";
                RideButtonIcon = "??";
            }
        }

        /// <summary>
        /// Toggles ride tracking (start/stop)
        /// </summary>
        [RelayCommand]
        private void ToggleRide()
        {
            try
            {
                if (IsRideActive)
                {
                    // Stop ride
                    bool saved = App.ProfileService.StopRide();

                    if (saved)
                    {
                        // Ride stopped successfully - data is saved
                        Debug.WriteLine($"? Ride saved: Distance={Distance:F2}km, Duration={TimeOnBike}, AvgSpeed={MediumSpeed:F1}km/h");
                    }
                }
                else
                {
                    // Start ride
                    App.ProfileService.StartRide();
                    Debug.WriteLine("?? Ride started from MainViewModel");
                }

                UpdateRideButtonState();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"? Error toggling ride: {ex.Message}");
            }
        }

        /// <summary>
        /// Navigates to saved rides page
        /// </summary>
        [RelayCommand]
        private void ViewSavedRides()
        {
            // This command is not used in Blazor - navigation happens via NavMenu
            Debug.WriteLine("ViewSavedRides command called (not implemented for Blazor)");
        }

        /// <summary>
        /// Logs out the user and returns to login page
        /// </summary>
        [RelayCommand]
        private void Logout()
        {
            try
            {
                // Stop active ride if running
                if (IsRideActive)
                {
                    App.ProfileService.StopRide();
                }

                // Logout
                App.LoginService.Logout();

                Debug.WriteLine("? User logged out from MainViewModel");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"? Error during logout: {ex.Message}");
            }
        }

        public void Dispose()
        {
            App.SensorService.SensorDataUpdated -= OnSensorDataUpdated;
            App.ProfileService.RideProfileUpdated -= OnProfileDataUpdated;
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
            Debug.WriteLine("?? MainViewModel disposed");
        }
    }
}
