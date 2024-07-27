using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using AndroidX.AppCompat.App;

namespace FloatingView
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            //SetContentView(Resource.Layout.activity_main);

            // Check for overlay permission and request if not granted
            if (!Android.Provider.Settings.CanDrawOverlays(this))
            {
                Intent intent = new Intent(Android.Provider.Settings.ActionManageOverlayPermission,
                    Android.Net.Uri.Parse("package:" + PackageName));
                StartActivityForResult(intent, 0);
            }
            else
            {
                StartFloatingButtonService();
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == 0 && Android.Provider.Settings.CanDrawOverlays(this))
            {
                StartFloatingButtonService();
            }
        }

        private void StartFloatingButtonService()
        {
            StartService(new Intent(this, typeof(FloatingButtonService)));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            StopService(new Intent(this, typeof(FloatingButtonService)));
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}