using System.Buffers.Binary;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace OneView.Services
{
    public class BluetoothService
    {
        private readonly IAdapter? _adapter;
        private readonly IDevice? _helmethDevice;
        private ICharacteristic? _helmetCharacteristic;

        public BluetoothService()
        {
            _adapter = CrossBluetoothLE.Current.Adapter;
        }
        public async Task<List<IDevice>> ScanForHelmets()
        {
            List<IDevice> devices = new List<IDevice>();



            void OnDeviceDiscovered(object? sender, DeviceEventArgs e)
            {
                if (e.Device.Name == "ESP32_Helmet" || e.Device.Name == "Smarthelm")
                {
                    devices.Add(e.Device);
                }
            }
            if (_adapter != null)
            {
                _adapter.DeviceDiscovered += OnDeviceDiscovered;
                try
                {
                    await _adapter.StartScanningForDevicesAsync();
                    await Task.Delay(5000);
                    await _adapter.StopScanningForDevicesAsync();
                }
                finally
                {
                    _adapter.DeviceDiscovered -= OnDeviceDiscovered;
                }
            }
            return devices;

        }
        public async Task ConnectToHelmet(IDevice device)
        {
            if (_adapter is null)
            {
                throw new InvalidOperationException("Bluetooth adapter is not available.");
            }

            await _adapter.ConnectToDeviceAsync(device);

            var services = await device.GetServicesAsync(); 

            foreach (var service in services)
            {
                var characteristics = await service.GetCharacteristicsAsync();

                foreach (var characteristic in characteristics)
                {
                    if (characteristic.CanWrite)
                    {
                        _helmetCharacteristic = characteristic;
                        return;
                    }
                }
            }
        }

        public async Task SendDataToHelmet(float speed, float leftAngle, float rightAngle, float battery)
        {
            if (_helmetCharacteristic is null) return;

            byte[] data = new byte[16];
            BinaryPrimitives.WriteSingleLittleEndian(data.AsSpan(0),  speed);
            BinaryPrimitives.WriteSingleLittleEndian(data.AsSpan(4),  leftAngle);
            BinaryPrimitives.WriteSingleLittleEndian(data.AsSpan(8),  rightAngle);
            BinaryPrimitives.WriteSingleLittleEndian(data.AsSpan(12), battery);

            await _helmetCharacteristic.WriteAsync(data);
        }

    }
}
