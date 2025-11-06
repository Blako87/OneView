using System.Diagnostics;
using Microsoft.Maui.ApplicationModel;

namespace OneView.Services
{
    public static class BluetoothPermissionService
    {
        public static async Task<bool> EnsureBluetoothPermissionsAsync()
        {
            try
            {
#if ANDROID
                // Step 1: Check if location services are enabled (hardware requirement)
                var locationManager = Android.App.Application.Context.GetSystemService(Android.Content.Context.LocationService) as Android.Locations.LocationManager;
                bool isLocationEnabled = locationManager?.IsLocationEnabled ?? false;

                if (!isLocationEnabled)
                {
                    Debug.WriteLine("? Location services are OFF - required for BLE scanning!");
                    return false;
                }
                else
                {
                    Debug.WriteLine("? Location services are ON");
                }

                // Step 2: Check Bluetooth permissions (Android 12+ requires BLUETOOTH_SCAN, BLUETOOTH_CONNECT)
                var status = await Permissions.CheckStatusAsync<Platforms.Android.Permissions.BluetoothPermissions>();

                if (status != PermissionStatus.Granted)
                {
                    Debug.WriteLine("?? Bluetooth permissions not granted, requesting...");
                    status = await Permissions.RequestAsync<Platforms.Android.Permissions.BluetoothPermissions>();

                    if (status != PermissionStatus.Granted)
                    {
                        Debug.WriteLine("? Bluetooth permissions denied by user");
                        return false;
                    }
                    else
                    {
                        Debug.WriteLine("? Bluetooth permissions granted");
                    }
                }
                else
                {
                    Debug.WriteLine("? Bluetooth permissions already granted");
                }
#else
                // iOS or other platforms - location only
                var locationStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                if (locationStatus != PermissionStatus.Granted)
                {
                    locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

                    if (locationStatus != PermissionStatus.Granted)
                    {
                        Debug.WriteLine("? Location permission denied");
                        return false;
                    }
                }
#endif

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"? Permission error: {ex.Message}");
                Debug.WriteLine($"? Stack trace: {ex.StackTrace}");
                return false;
            }
        }
    }
}
