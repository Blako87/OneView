using System.Buffers.Binary;
using System.Diagnostics;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;
using Plugin.BLE.Abstractions;

namespace OneView.Services
{
    public class BluetoothService
    {
        private readonly IAdapter? _adapter;
        private readonly IBluetoothLE _ble;
        private ICharacteristic? _helmetCharacteristic;
        private IDevice? _connectedHelmetDevice;

        // ESP32 Helmet BLE Service and Characteristic UUIDs
        private readonly Guid _helmetServiceGuid = Guid.Parse("19B10000-E8F2-537E-4F6C-D104768A1214");
        private readonly Guid _helmetCharacteristicGuid = Guid.Parse("19B10001-E8F2-537E-4F6C-D104768A1214");

        // Dashboard tracking properties
        public int DataSentCount { get; private set; } = 0;
        public DateTime? LastDataSentTime { get; private set; }
        public string LastError { get; private set; } = "";
        public string CharacteristicInfo { get; private set; } = "";
        public int DevicesFoundDuringScan { get; private set; } = 0;

        public BluetoothService()
        {
            _adapter = CrossBluetoothLE.Current.Adapter;
            _ble = CrossBluetoothLE.Current;
            _ble.StateChanged += OnBluetoothStateChanged;
        }

        private void OnBluetoothStateChanged(object? sender, BluetoothStateChangedArgs e)
        {
            Debug.WriteLine($"🔵 Bluetooth state: {e.NewState}");

            if (e.NewState == BluetoothState.Off && _connectedHelmetDevice != null)
            {
                _ = DisconnectHelmet();
            }
        }

        public bool IsBluetoothAvailable()
        {
            return _ble.State == BluetoothState.On;
        }

        private async Task<bool> EnsureBluetoothReadyAsync()
        {
            if (_adapter is null)
            {
                Debug.WriteLine("❌ Bluetooth adapter is null");
                LastError = "Bluetooth adapter ist null";
                return false;
            }

            if (!IsBluetoothAvailable())
            {
                Debug.WriteLine("❌ Bluetooth is off");
                LastError = "Bluetooth ist AUS";
                return false;
            }

            // Check all Bluetooth permissions (includes location services check on Android)
            if (!await BluetoothPermissionService.EnsureBluetoothPermissionsAsync())
            {
                Debug.WriteLine("❌ Bluetooth permissions or location services not ready");
                LastError = "Standortdienste oder Bluetooth-Berechtigungen fehlen";
                return false;
            }

            return true;
        }

        public async Task DisconnectHelmet()
        {
            if (_connectedHelmetDevice != null && _adapter != null)
            {
                try
                {
                    await _adapter.DisconnectDeviceAsync(_connectedHelmetDevice);
                    Debug.WriteLine($"🔌 Disconnected from {_connectedHelmetDevice.Name}");
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"❌ Disconnect error: {e.Message}");
                    LastError = $"Disconnect error: {e.Message}";
                }
                finally
                {
                    _connectedHelmetDevice = null;
                    _helmetCharacteristic = null;
                    DataSentCount = 0;
                    CharacteristicInfo = "";
                }
            }
        }

        public async Task<List<IDevice>> ScanForHelmets()
        {
            List<IDevice> devices = new();
            DevicesFoundDuringScan = 0;

            if (!await EnsureBluetoothReadyAsync())
            {
                return devices;
            }

            void OnDeviceDiscovered(object? sender, DeviceEventArgs e)
            {
                DevicesFoundDuringScan++;

                // Log EVERY device found
                var deviceName = e.Device?.Name ?? "Unknown";
                Debug.WriteLine($"🔍 Device #{DevicesFoundDuringScan}: Name='{deviceName}', ID={e.Device?.Id}");

                // Check for helmet with case-insensitive matching and partial name matching
                if (!string.IsNullOrWhiteSpace(e.Device?.Name))
                {
                    string name = e.Device.Name.ToLowerInvariant();

                    if (name.Contains("helmet") ||
                        name.Contains("smarthelm") ||
                        name.Contains("esp32") ||
                        name == "helmet_01" ||
                        name == "smarthelm")
                    {
                        if (!devices.Any(d => d.Id == e.Device.Id))
                        {
                            devices.Add(e.Device);
                            Debug.WriteLine($"✅ HELMET FOUND: {e.Device.Name}");
                        }
                    }
                }
            }

            _adapter!.DeviceDiscovered += OnDeviceDiscovered;
            try
            {
                Debug.WriteLine("🔍 Starting BLE scan (looking for ANY device)...");
                Debug.WriteLine("📍 Scanning for devices with names containing: helmet, smarthelm, esp32");

                // Scan without filters to find ALL devices
                await _adapter.StartScanningForDevicesAsync();
                await Task.Delay(8000); // Increased scan time to 8 seconds

                Debug.WriteLine($"🔍 Scan complete. Found {DevicesFoundDuringScan} total device(s), {devices.Count} helmet(s)");

                if (DevicesFoundDuringScan == 0)
                {
                    Debug.WriteLine("⚠️ NO DEVICES FOUND AT ALL!");
                    Debug.WriteLine("⚠️ Possible issues:");
                    Debug.WriteLine("   1. Location services are OFF");
                    Debug.WriteLine(" 2. Location permission not granted");
                    Debug.WriteLine("   3. Bluetooth scan permission not granted (Android 12+)");
                    LastError = "Keine Geräte gefunden. Prüfe Standortdienste und Berechtigungen.";
                }
                else if (devices.Count == 0)
                {
                    Debug.WriteLine($"⚠️ Found {DevicesFoundDuringScan} devices but NONE match helmet filter");
                    Debug.WriteLine("⚠️ Check ESP32 device name in Bluetooth settings and update filter");
                    LastError = $"{DevicesFoundDuringScan} Geräte gefunden, aber kein Helm. Prüfe ESP32 Name.";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Scan error: {ex.Message}");
                Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                LastError = $"Scan error: {ex.Message}";
            }
            finally
            {
                try
                {
                    await _adapter.StopScanningForDevicesAsync();
                }
                catch { }

                _adapter.DeviceDiscovered -= OnDeviceDiscovered;
            }

            return devices;
        }

        public async Task<bool> ConnectToHelmet(IDevice device)
        {
            if (!await EnsureBluetoothReadyAsync())
            {
                return false;
            }

            try
            {
                if (_adapter!.IsScanning)
                {
                    await _adapter.StopScanningForDevicesAsync();
                }

                Debug.WriteLine($"🔗 Connecting to {device.Name}...");
                await _adapter.ConnectToDeviceAsync(device);
                _connectedHelmetDevice = device;

                // Request larger MTU for data transfer
                try
                {
                    var mtu = await device.RequestMtuAsync(512);
                    Debug.WriteLine($"📏 MTU set to: {mtu}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"⚠️ MTU request failed: {ex.Message}");
                }

                Debug.WriteLine($"🔍 Discovering services...");
                var services = await device.GetServicesAsync();
                Debug.WriteLine($"📋 Found {services.Count} service(s)");

                // List all services for debugging
                foreach (var svc in services)
                {
                    Debug.WriteLine($"   Service: {svc.Id}");
                }

                // Try to find the specific helmet service by GUID
                var helmetService = services.FirstOrDefault(s => s.Id == _helmetServiceGuid);

                if (helmetService != null)
                {
                    Debug.WriteLine($"✅ Found helmet service: {helmetService.Id}");
                    var characteristics = await helmetService.GetCharacteristicsAsync();
                    Debug.WriteLine($"📋 Found {characteristics.Count} characteristic(s) in helmet service");

                    // List all characteristics
                    foreach (var ch in characteristics)
                    {
                        Debug.WriteLine($"   Char: {ch.Id}, CanWrite={ch.CanWrite}, CanRead={ch.CanRead}, Properties={ch.Properties}");
                    }

                    var helmetChar = characteristics.FirstOrDefault(c => c.Id == _helmetCharacteristicGuid);

                    if (helmetChar != null)
                    {
                        _helmetCharacteristic = helmetChar;
                        CharacteristicInfo = $"Service: {helmetService.Id}, Char: {helmetChar.Id}, Properties: {helmetChar.Properties}";
                        Debug.WriteLine($"✅ Connected to {device.Name}");
                        Debug.WriteLine($"✅ Using characteristic: {helmetChar.Id}");
                        Debug.WriteLine($"✅ Properties: {helmetChar.Properties}");
                        Debug.WriteLine($"✅ CanWrite: {helmetChar.CanWrite}");
                        LastError = "";
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine($"❌ Characteristic {_helmetCharacteristicGuid} not found in helmet service");
                    }
                }
                else
                {
                    Debug.WriteLine($"❌ Helmet service {_helmetServiceGuid} not found");
                }

                // Fallback: search for any writable characteristic
                Debug.WriteLine("⚠️ Trying fallback: searching for any writable characteristic...");
                foreach (var service in services)
                {
                    Debug.WriteLine($"   Checking service: {service.Id}");
                    var characteristics = await service.GetCharacteristicsAsync();

                    foreach (var characteristic in characteristics)
                    {
                        Debug.WriteLine($"      Char: {characteristic.Id}, CanWrite={characteristic.CanWrite}, Properties={characteristic.Properties}");

                        if (characteristic.CanWrite)
                        {
                            _helmetCharacteristic = characteristic;
                            CharacteristicInfo = $"FALLBACK - Service: {service.Id}, Char: {characteristic.Id}, Properties: {characteristic.Properties}";
                            Debug.WriteLine($"⚠️ Using fallback characteristic:");
                            Debug.WriteLine($"   Service: {service.Id}");
                            Debug.WriteLine($"   Char: {characteristic.Id}");
                            Debug.WriteLine($"   Properties: {characteristic.Properties}");
                            LastError = "Using fallback characteristic (GUID mismatch)";
                            return true;
                        }
                    }
                }

                Debug.WriteLine("❌ No writable characteristic found at all!");
                LastError = "No writable characteristic found";
                return false;
            }
            catch (DeviceConnectionException e)
            {
                Debug.WriteLine($"❌ Connection failed: {e.Message}");
                LastError = $"Connection failed: {e.Message}";
                return false;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"❌ Connection error: {e.Message}");
                Debug.WriteLine($"❌ Stack trace: {e.StackTrace}");
                LastError = $"Connection error: {e.Message}";
                return false;
            }
        }

        public async Task<bool> SendDataToHelmet(float speed, float leftAngle, float rightAngle, float battery)
        {
            if (_helmetCharacteristic is null)
            {
                Debug.WriteLine("❌ No helmet characteristic");
                LastError = "No helmet characteristic";
                return false;
            }

            if (!IsBluetoothAvailable())
            {
                Debug.WriteLine("❌ Bluetooth is off");
                LastError = "Bluetooth is off";
                return false;
            }

            try
            {
                byte[] data = new byte[16];
                BinaryPrimitives.WriteSingleLittleEndian(data.AsSpan(0, 4), speed);
                BinaryPrimitives.WriteSingleLittleEndian(data.AsSpan(4, 4), leftAngle);
                BinaryPrimitives.WriteSingleLittleEndian(data.AsSpan(8, 4), rightAngle);
                BinaryPrimitives.WriteSingleLittleEndian(data.AsSpan(12, 4), battery);

                // Write data to characteristic
                await _helmetCharacteristic.WriteAsync(data);

                DataSentCount++;
                LastDataSentTime = DateTime.Now;
                LastError = "";

                Debug.WriteLine($"✅ DATA SENT #{DataSentCount} at {LastDataSentTime:HH:mm:ss.fff}");
                Debug.WriteLine($"   Speed={speed:F1} km/h, Left={leftAngle:F1}°, Right={rightAngle:F1}°, Bat={battery:F0}%");
                Debug.WriteLine($"   Hex: {BitConverter.ToString(data)}");
                Debug.WriteLine($"   Dec: [{data[0]},{data[1]},{data[2]},{data[3]}] [{data[4]},{data[5]},{data[6]},{data[7]}] [{data[8]},{data[9]},{data[10]},{data[11]}] [{data[12]},{data[13]},{data[14]},{data[15]}]");

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"❌ Send error: {e.Message}");
                Debug.WriteLine($"❌ Stack trace: {e.StackTrace}");
                LastError = $"Send error: {e.Message}";
                return false;
            }
        }

        public bool IsHelmetConnected()
        {
            return _connectedHelmetDevice?.State == Plugin.BLE.Abstractions.DeviceState.Connected;
        }
    }
}
