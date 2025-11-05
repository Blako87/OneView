using OneView.Services;
using OneView.Models;
using Plugin.BLE.Abstractions.Contracts;

namespace OneView
{
    public partial class App : Application
    {
        public static SensorService SensorService { get; private set; } = null!;
        private readonly Sensordata Sensordata = new();
        public static BluetoothService BluetoothService { get; set; } = new BluetoothService();
        private readonly bool _isActive = BluetoothService.IsBlueetoothAvailabale();
        public App()
        {
            InitializeComponent();
            SensorService = new SensorService();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = base.CreateWindow(activationState);
            window.Page = new MainPage();
            return window;
        }

        protected override void OnSleep()
        {
            base.OnSleep();
            SensorService.StopWatchingBattery();
            SensorService.StopAccelerometer();
            BluetoothService.DisconnectHelmet();
        }
        protected override async void OnResume()
        {
            base.OnResume();
            SensorService.StartWatchingBattery();
            SensorService.StartAccelerometer();
            List<IDevice> devices = await BluetoothService.ScanForHelmets();
            if(devices != null)
            {
                await BluetoothService.ConnectToHelmet(devices[0]);
                await BluetoothService.SendDataToHelmet(Sensordata.SpeedKmh, Sensordata.InclineAngleDegLeft, Sensordata.InclineAngleDegRight, Sensordata.BatteryPercent);
            }
            

        }
        protected override async void OnStart()
        {
            base.OnStart();
            if (_isActive)
            {

                List<IDevice> devices = await BluetoothService.ScanForHelmets();
                if (devices != null)
                {
                    await BluetoothService.ConnectToHelmet(devices[0]);
                    await BluetoothService.SendDataToHelmet(Sensordata.SpeedKmh, Sensordata.InclineAngleDegLeft, Sensordata.InclineAngleDegRight, Sensordata.BatteryPercent);
                }
            }
            else
            {
                throw new InvalidOperationException("Turn your Bluetooth On.");
            }

        }
    }
}
