using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;

namespace FloatingView
{
    [Service]
    public class FloatingButtonService : Service
    {
        private WindowManagerLayoutParams layoutParams;
        private IWindowManager windowManager;
        private View floatingView;

        public override void OnCreate()
        {
            base.OnCreate();

            floatingView = LayoutInflater.From(this).Inflate(Resource.Layout.floating_button, null);

            layoutParams = new WindowManagerLayoutParams(
                WindowManagerLayoutParams.WrapContent,
                WindowManagerLayoutParams.WrapContent,
                WindowManagerTypes.ApplicationOverlay,
                WindowManagerFlags.NotFocusable,
                Format.Translucent)
            {
                Gravity = GravityFlags.Center | GravityFlags.Center,
                X = 0,
                Y = 100
            };

            windowManager = GetSystemService(WindowService).JavaCast<IWindowManager>();
            windowManager.AddView(floatingView, layoutParams);

            floatingView.FindViewById<Button>(Resource.Id.floating_button).Click += OnFloatingButtonClick;
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (floatingView != null)
            {
                windowManager.RemoveView(floatingView);
            }
        }

        private void OnFloatingButtonClick(object sender, EventArgs e)
        {
            Toast.MakeText(this, "Emergency Button Clicked", ToastLength.Short).Show();
            // Handle the button click action here
        }
    }
}