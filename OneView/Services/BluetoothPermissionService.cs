using System.Diagnostics;
using Microsoft.Maui.ApplicationModel;

namespace OneView.Services
{
    /// <summary>
    /// Handles Bluetooth and Location permission requests for Android
    /// Centralizes all permission checks and requests in one place
    /// Verifies hardware prerequisites (location services) before requesting permissions
    /// Required for BLE scanning which needs location permissions on Android 6+
    /// </summary>
    public static class BluetoothPermissionService
    {
        /// <summary>
        /// Ensures all Bluetooth-related permissions and prerequisites are satisfied
        /// Performs the following checks in order:
        /// 1. (Android) Verifies location services are enabled (hardware requirement for BLE)
        /// 2. Requests BLUETOOTH_SCAN, BLUETOOTH_CONNECT (Android 12+) or ACCESS_FINE_LOCATION (older)
        /// 3. (iOS) Requests LocationWhenInUse permission
        /// </summary>
        /// <returns>True if all permissions granted and prerequisites met, false otherwise</returns>
        public static async Task<bool> EnsureBluetoothPermissionsAsync()
        {
            try
            {
#if ANDROID
                // STEP 1: Check if location services are enabled in phone settings
                // This is a HARDWARE requirement - BLE scanning requires GPS/location to be ON
                // Even with permissions, BLE won't work if location is disabled
                var locationManager = Android.App.Application.Context.GetSystemService(Android.Content.Context.LocationService) as Android.Locations.LocationManager;
                bool isLocationEnabled = locationManager?.IsLocationEnabled ?? false;

                if (!isLocationEnabled)
                {
                    Debug.WriteLine("? Location services are OFF - required for BLE scanning!");
                    // User must manually enable location in phone settings
                    return false;
                }
                else
                {
                    Debug.WriteLine("? Location services are ON");
                }

                // STEP 2: Check Bluetooth runtime permissions
                // Android 12+ requires BLUETOOTH_SCAN and BLUETOOTH_CONNECT
                // Older versions require ACCESS_FINE_LOCATION and ACCESS_COARSE_LOCATION
                // These are defined in Platforms/Android/Permissions/BluetoothPermissions.cs
                var status = await Permissions.CheckStatusAsync<Platforms.Android.Permissions.BluetoothPermissions>();

                if (status != PermissionStatus.Granted)
                {
                    Debug.WriteLine("?? Bluetooth permissions not granted, requesting...");
                    
                    // Show permission request dialog to user
                    status = await Permissions.RequestAsync<Platforms.Android.Permissions.BluetoothPermissions>();

                    if (status != PermissionStatus.Granted)
                    {
                        // User denied permission - cannot use Bluetooth
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
                // iOS and other platforms - only need location permission
                // iOS doesn't require location services to be ON for BLE
                var locationStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                if (locationStatus != PermissionStatus.Granted)
                {
                    // Request location permission
                    locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

                    if (locationStatus != PermissionStatus.Granted)
                    {
                        Debug.WriteLine("? Location permission denied");
                        return false;
                    }
                }
#endif

                // All checks passed!
                return true;
            }
            catch (Exception ex)
            {
                // Permission request failed (shouldn't happen, but handle gracefully)
                Debug.WriteLine($"? Permission error: {ex.Message}");
                Debug.WriteLine($"? Stack trace: {ex.StackTrace}");
                return false;
            }
        }
    }
}
