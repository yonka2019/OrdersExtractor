using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using System.Threading.Tasks;

namespace OrdersExtractor.Activities
{
    [Activity(Label = "@string/app_name",
            MainLauncher = true,
            NoHistory = true,
            Icon = "@mipmap/ic_launcher")]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnResume()
        {
            base.OnResume();
            SetContentView(Resource.Layout.splash_screen);
            RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;

            Task startupWork = new Task(async () => { await SimulateStartupAsync(); });
            startupWork.Start();
        }

        private async Task SimulateStartupAsync()
        {
            await Task.Delay(1000);

            Intent preActivity = new Intent(Application.Context, typeof(MainActivity));
            StartActivity(preActivity);
        }
    }
}