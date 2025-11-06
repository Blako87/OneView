using Android.OS;
using Microsoft.Maui.ApplicationModel;

namespace OneView.Platforms.Android.Permissions;

public class BluetoothPermissions : Microsoft.Maui.ApplicationModel.Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        Build.VERSION.SdkInt >= BuildVersionCodes.S
  ? new[]
  {
       (global::Android.Manifest.Permission.BluetoothScan, true),
        (global::Android.Manifest.Permission.BluetoothConnect, true),
          (global::Android.Manifest.Permission.AccessFineLocation, true)
            }
        : new[]
        {
                (global::Android.Manifest.Permission.AccessFineLocation, true),
       (global::Android.Manifest.Permission.AccessCoarseLocation, true)
      };
}
