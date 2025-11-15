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

        #endregion

        public MainViewModel()
        {
            // Load username
            Username = App.LoginService.GetUsername();

            // Start auto-refresh timer (every 200ms for smooth UI updates)
            _refreshTimer = new System.Timers.Timer(200);
            _refreshTimer.Elapsed += (s, e) => RefreshData();
            _refreshTimer.AutoReset = true;
            _refreshTimer.Start();

            Debug.WriteLine("? MainViewModel initialized");
        }

        /// <summary>
        /// Refreshes all data from services
        /// Called automatically every 200ms
        /// </summary>
        private void RefreshData()
        {
            try
            {
                // Get sensor data
                var sensorData = App.SensorService.CurrentSensorData;
                Speed = sensorData.SpeedKmh;
                BatteryPercent = sensorData.BatteryPercent;
                InclineLeft = sensorData.InclineAngleDegLeft;
                InclineRight = sensorData.InclineAngleDegRight;

                // Get ride profile data
                var profile = App.ProfileService.CurrentRideProfile;
                Distance = profile.Distance;
                MediumSpeed = profile.MediumSpeed;
                MaxSpeed = profile.MaxSpeed;
                MinInclineLeft = profile.MinInclineAngleLeft;
                MaxInclineLeft = profile.MaxInclineAngleLeft;
                MinInclineRight = profile.MinInclineAngleRight;
                MaxInclineRight = profile.MaxInclineAngleRight;
                TimeOnBike = App.ProfileService.GetRideDurationFormatted();

                // Get Bluetooth status
                IsBluetoothOn = App.BluetoothService.IsBluetoothAvailable();
                IsHelmetConnected = App.BluetoothService.IsHelmetConnected();
                PacketsSent = App.BluetoothService.DataSentCount;
                
                var lastTime = App.BluetoothService.LastDataSentTime;
                LastTransmission = lastTime?.ToString("HH:mm:ss") ?? "Nie";

                // Update ride status
                IsRideActive = App.ProfileService.IsRideActive;
                UpdateRideButtonState();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"? Error refreshing data: {ex.Message}");
            }
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
        private async Task ToggleRide()
        {
            try
            {
                if (IsRideActive)
                {
                    // Stop ride
                    bool saved = App.ProfileService.StopRide();
                    
                    if (saved)
                    {
                        await Shell.Current.DisplayAlert(
                            "Fahrt Beendet",
                            $"Distanz: {Distance:F2} km\n" +
                            $"Dauer: {TimeOnBike}\n" +
                            $"Durchschn.: {MediumSpeed:F1} km/h\n" +
                            $"Max: {MaxSpeed:F1} km/h",
                            "OK");
                    }
                }
                else
                {
                    // Start ride
                    App.ProfileService.StartRide();
                    Debug.WriteLine("? Ride started");
                }

                UpdateRideButtonState();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"? Error toggling ride: {ex.Message}");
                await Shell.Current.DisplayAlert("Fehler", ex.Message, "OK");
            }
        }

        /// <summary>
        /// Navigates to saved rides page
        /// </summary>
        [RelayCommand]
        private async Task ViewSavedRides()
        {
            await Shell.Current.GoToAsync("///SavedRidesPage");
        }

        /// <summary>
        /// Logs out the user and returns to login page
        /// </summary>
        [RelayCommand]
        private async Task Logout()
        {
            bool confirm = await Shell.Current.DisplayAlert(
                "Abmelden",
                "Möchten Sie sich wirklich abmelden?",
                "Ja",
                "Nein");

            if (confirm)
            {
                // Stop active ride if running
                if (IsRideActive)
                {
                    App.ProfileService.StopRide();
                }

                // Logout
                App.LoginService.Logout();

                // Navigate to login
                await Shell.Current.GoToAsync("///LoginPage");
            }
        }

        public void Dispose()
        {
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
            Debug.WriteLine("?? MainViewModel disposed");
        }
    }
}
