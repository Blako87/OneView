using OneView.Services;
using OneView.Models;
using Plugin.BLE.Abstractions.Contracts;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace OneView
{
    /// <summary>
    /// Main application class - Entry point and global service container
    /// Manages application lifecycle (OnStart, OnSleep, OnResume)
    /// Provides static access to all services via App.ServiceName pattern
    /// Handles:
    /// - Sensor initialization (Battery, Accelerometer, GPS)
    /// - Bluetooth connection and data transmission to ESP32 helmet
    /// - Login state restoration
    /// - Automatic reconnection on app resume
    /// </summary>
    public partial class App : Application
    {
        #region Global Services (Accessible via App.ServiceName)

        /// <summary>
        /// Hardware sensor service (Battery, Accelerometer, GPS)
        /// Access via: App.SensorService.CurrentSensorData
        /// </summary>
        public static SensorService SensorService { get; private set; } = null!;

        /// <summary>
        /// Bluetooth Low Energy service for ESP32 helmet communication
        /// Access via: App.BluetoothService.SendDataToHelmet(...)
        /// </summary>
        public static BluetoothService BluetoothService { get; set; } = new BluetoothService();

        /// <summary>
        /// Ride tracking and statistics service
        /// Access via: App.ProfileService.StartRide() / StopRide()
        /// </summary>
        public static ProfileService ProfileService { get; private set; } = null!;

        /// <summary>
        /// User authentication and login service
        /// Access via: App.LoginService.Login(...) / IsLoggedIn()
        /// </summary>
        public static LoginService LoginService { get; private set; } = null!;

        #endregion

        #region Bluetooth Data Transmission

        /// <summary>
        /// Timer that sends sensor data to ESP32 helmet every 500ms
        /// Transmits: Speed, Left/Right tilt angles, Battery level
        /// </summary>
        private System.Timers.Timer? _dataSendTimer;
        
        /// <summary>
        /// Interval for sending data to helmet (500 milliseconds = 2 updates per second)
        /// </summary>
        private const int DataSendIntervalMs = 500;

        #endregion

        /// <summary>
        /// Application constructor - Initializes all services and UI
        /// Called once when app first launches
        /// </summary>
        public App()
        {
            InitializeComponent();
            
            // Initialize all global services
            SensorService = new SensorService();
            ProfileService = new ProfileService();
            LoginService = new LoginService();
            
            // Use AppShell for navigation (Blazor handles internal routing)
            MainPage = new AppShell();

            // Setup timer for periodic Bluetooth data transmission
            SetupDataSendTimer();
            
            // Try to restore previous login session from disk
            bool wasLoggedIn = LoginService.LoadLogin();
            Debug.WriteLine($"Login restored: {wasLoggedIn}");
            
            // Blazor routing will handle navigation to login or home page
        }

        /// <summary>
        /// Sets up the timer for periodic Bluetooth data transmission
        /// Timer fires every 500ms to send sensor data to ESP32 helmet
        /// Timer is created here but only started when helmet connects
        /// </summary>
        private void SetupDataSendTimer()
        {
            _dataSendTimer = new System.Timers.Timer(DataSendIntervalMs);
            
            // Attach async event handler for timer tick
            _dataSendTimer.Elapsed += async (sender, e) => await SendDataToHelmetAsync();
            
            // Keep firing every 500ms (don't stop after first tick)
            _dataSendTimer.AutoReset = true;
        }

        /// <summary>
        /// Sends current sensor data to ESP32 helmet via Bluetooth
        /// Called automatically every 500ms by _dataSendTimer
        /// Only sends if helmet is connected
        /// Transmits 16 bytes: [Speed(4)][LeftAngle(4)][RightAngle(4)][Battery(4)]
        /// </summary>
        private async Task SendDataToHelmetAsync()
        {
            // Don't send if helmet isn't connected
            if (!BluetoothService.IsHelmetConnected())
            {
                return;
            }

            try
            {
                // Get latest sensor data
                var sensorData = SensorService.CurrentSensorData;
                
                // Send to ESP32 helmet via Bluetooth
                await BluetoothService.SendDataToHelmet(
                    sensorData.SpeedKmh,
                    sensorData.InclineAngleDegLeft,
                    sensorData.InclineAngleDegRight,
                    sensorData.BatteryPercent);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sending data: {ex.Message}");
            }
        }

        #region Application Lifecycle Events

        /// <summary>
        /// Called when app enters background (user switches to another app)
        /// Stops all sensors and disconnects Bluetooth to save battery
        /// Automatically saves active ride data
        /// </summary>
        protected override void OnSleep()
        {
            base.OnSleep();
            
            // Stop sending data to helmet
            StopDataSending();
            
            // Stop all sensors to save battery
            SensorService.StopWatchingBattery();
            SensorService.StopAccelerometer();
            SensorService.StopWatchingGps();

            // Stop active ride if running (auto-saves ride data)
            if (ProfileService.IsRideActive)
            {
                Debug.WriteLine("⚠️ Ride stopped due to app sleep");
                ProfileService.StopRide();
            }

            // Disconnect from Bluetooth helmet (run in background to not block UI)
            Task.Run(async () =>
            {
                try
                {
                    await BluetoothService.DisconnectHelmet();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error on sleep: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Called when app returns to foreground (user switches back to app)
        /// Restarts all sensors and attempts to reconnect to Bluetooth helmet
        /// </summary>
        protected override async void OnResume()
        {
            base.OnResume();
            
            // Restart all sensors
            SensorService.StartWatchingBattery();
            SensorService.StartAccelerometer();
            await SensorService.StartWatchingGps();

            // Try to reconnect to helmet
            await ReconnectToHelmetAsync();
        }

        /// <summary>
        /// Called when app first starts (cold start)
        /// Initializes all sensors, checks permissions, and connects to helmet
        /// </summary>
        protected override async void OnStart()
        {
            base.OnStart();

            // Check if Bluetooth is available on this device
            if (!BluetoothService.IsBluetoothAvailable())
            {
                Debug.WriteLine("Bluetooth is not available");

                // Show alert to user (run on UI thread)
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.DisplayAlert(
                        "Bluetooth Required",
                        "Please turn on Bluetooth to connect to your helmet.",
                        "OK");
                });
                return;
            }

            // Start all sensors
            SensorService.StartWatchingBattery();
            SensorService.StartAccelerometer();
            
            // Start GPS (may fail if permissions denied)
            bool gpsStarted = await SensorService.StartWatchingGps();
            if (!gpsStarted)
            {
                Debug.WriteLine("⚠️ GPS could not be started - check permissions");
            }
            else
            {
                Debug.WriteLine("✅ GPS started successfully");
            }

            // Scan for and connect to helmet
            await ReconnectToHelmetAsync();
        }

        #endregion

        #region Bluetooth Connection Management

        /// <summary>
        /// Scans for and connects to ESP32 helmet via Bluetooth
        /// Called on app start and resume
        /// Automatically starts data transmission if connection successful
        /// </summary>
        private async Task ReconnectToHelmetAsync()
        {
            try
            {
                // Check if already connected (prevents duplicate connections)
                if (BluetoothService.IsHelmetConnected())
                {
                    Debug.WriteLine("Helmet already connected");
                    StartDataSending();
                    return;
                }

                // Scan for helmets (looks for devices named "Helmet_01" or "Smarthelm")
                // Scan duration: 8 seconds
                List<IDevice> devices = await BluetoothService.ScanForHelmets();

                if (devices == null || devices.Count == 0)
                {
                    Debug.WriteLine("No helmets found");
                    return;
                }

                // Connect to first helmet found
                var device = devices[0];
                bool connected = await BluetoothService.ConnectToHelmet(device);

                if (connected)
                {
                    Debug.WriteLine("Connected to helmet - starting data transmission");

                    // Send initial data immediately
                    await SendDataToHelmetAsync();

                    // Start continuous data sending (every 500ms)
                    StartDataSending();
                }
                else
                {
                    Debug.WriteLine("Failed to connect to helmet");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reconnecting: {ex.Message}");
            }
        }

        /// <summary>
        /// Starts the periodic data transmission timer
        /// Data is sent to helmet every 500ms
        /// </summary>
        private void StartDataSending()
        {
            if (_dataSendTimer != null && !_dataSendTimer.Enabled)
            {
                _dataSendTimer.Start();
                Debug.WriteLine("Data sending started");
            }
        }

        /// <summary>
        /// Stops the periodic data transmission timer
        /// Called when app sleeps or helmet disconnects
        /// </summary>
        private void StopDataSending()
        {
            if (_dataSendTimer != null && _dataSendTimer.Enabled)
            {
                _dataSendTimer.Stop();
                Debug.WriteLine("Data sending stopped");
            }
        }

        #endregion
    }
}
