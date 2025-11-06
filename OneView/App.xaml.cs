using OneView.Services;
using OneView.Models;
using Plugin.BLE.Abstractions.Contracts;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace OneView
{
    public partial class App : Application
    {
        public static SensorService SensorService { get; private set; } = null!;
        public static BluetoothService BluetoothService { get; set; } = new BluetoothService();

        private System.Timers.Timer? _dataSendTimer;
        private const int DataSendIntervalMs = 500; // Send data every 500ms

        public App()
        {
            InitializeComponent();
            SensorService = new SensorService();
            MainPage = new MainPage();

            // Setup data sending timer
            SetupDataSendTimer();
        }

        private void SetupDataSendTimer()
        {
            _dataSendTimer = new System.Timers.Timer(DataSendIntervalMs);
            _dataSendTimer.Elapsed += async (sender, e) => await SendDataToHelmetAsync();
            _dataSendTimer.AutoReset = true;
        }

        private async Task SendDataToHelmetAsync()
        {
            if (!BluetoothService.IsHelmetConnected())
            {
                return;
            }

            try
            {
                var sensorData = SensorService.CurrentSensorData;
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

        protected override void OnSleep()
        {
            base.OnSleep();
            StopDataSending();
            SensorService.StopWatchingBattery();
            SensorService.StopAccelerometer();

            // Fire and forget - don't block the UI thread
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

        protected override async void OnResume()
        {
            base.OnResume();
            SensorService.StartWatchingBattery();
            SensorService.StartAccelerometer();

            await ReconnectToHelmetAsync();
        }

        protected override async void OnStart()
        {
            base.OnStart();

            // Don't throw - handle gracefully
            if (!BluetoothService.IsBluetoothAvailable())
            {
                Debug.WriteLine("Bluetooth is not available");

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current!.MainPage!.DisplayAlert(
                        "Bluetooth Required",
                        "Please turn on Bluetooth to connect to your helmet.",
                        "OK");
                });
                return;
            }

            // Start sensors immediately
            SensorService.StartWatchingBattery();
            SensorService.StartAccelerometer();

            // Connect and start sending data
            await ReconnectToHelmetAsync();
        }

        private async Task ReconnectToHelmetAsync()
        {
            try
            {
                // Check if already connected
                if (BluetoothService.IsHelmetConnected())
                {
                    Debug.WriteLine("Helmet already connected");
                    StartDataSending();
                    return;
                }

                List<IDevice> devices = await BluetoothService.ScanForHelmets();

                if (devices == null || devices.Count == 0)
                {
                    Debug.WriteLine("No helmets found");
                    return;
                }

                var device = devices[0];
                bool connected = await BluetoothService.ConnectToHelmet(device);

                if (connected)
                {
                    Debug.WriteLine("Connected to helmet - starting data transmission");

                    // Send initial data immediately
                    await SendDataToHelmetAsync();

                    // Start continuous data sending
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

        private void StartDataSending()
        {
            if (_dataSendTimer != null && !_dataSendTimer.Enabled)
            {
                _dataSendTimer.Start();
                Debug.WriteLine("Data sending started");
            }
        }

        private void StopDataSending()
        {
            if (_dataSendTimer != null && _dataSendTimer.Enabled)
            {
                _dataSendTimer.Stop();
                Debug.WriteLine("Data sending stopped");
            }
        }
    }
}
