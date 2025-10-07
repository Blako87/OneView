using Microsoft.Maui.Controls;
using OneView.Services;

#if ANDROID
using Android.OS.Strictmode;
#endif
namespace OneView
{
    public partial class App : Application
    {
        public static SensorService SensorService { get; private set; }
        public App()
        {
            InitializeComponent();
            SensorService = new SensorService();

            MainPage = new MainPage();
            
        }
        protected override void OnSleep()
        {
                base.OnSleep();
                SensorService.StopWatchingBattery();
                SensorService.StopAccelerometer();
        }
        protected override void OnResume()
        {
                base.OnResume();
                SensorService.StartWatchingBattery();
                SensorService.StartAccelerometer();
        }
      
    }
}
