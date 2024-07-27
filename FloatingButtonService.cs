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
    public class FloatingButtonService : Service, View.IOnTouchListener
    {
        private WindowManagerLayoutParams layoutParams;
        private IWindowManager windowManager;
        private View floatingView;
        private float initialTouchX;
        private float initialTouchY;
        private int initialX;
        private int initialY;

        public override void OnCreate()
        {
            base.OnCreate();

            floatingView = LayoutInflater.From(this).Inflate(Resource.Layout.floating_button, null);
            floatingView.SetOnTouchListener(this);

            layoutParams = new WindowManagerLayoutParams(
                WindowManagerLayoutParams.WrapContent,
                WindowManagerLayoutParams.WrapContent,
                WindowManagerTypes.ApplicationOverlay,
                WindowManagerFlags.NotFocusable,
                Format.Translucent)
            {
                Gravity = GravityFlags.Top | GravityFlags.Left,
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

        public bool OnTouch(View v, MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    initialX = layoutParams.X;
                    initialY = layoutParams.Y;
                    initialTouchX = e.RawX;
                    initialTouchY = e.RawY;
                    return true;

                case MotionEventActions.Up:
                    // If the button is dragged to the edge, stick it there
                    if (layoutParams.X < (windowManager.DefaultDisplay.Width / 2))
                    {
                        layoutParams.X = 0;
                    }
                    else
                    {
                        layoutParams.X = windowManager.DefaultDisplay.Width;
                    }
                    windowManager.UpdateViewLayout(floatingView, layoutParams);
                    return true;

                case MotionEventActions.Move:
                    layoutParams.X = initialX + (int)(e.RawX - initialTouchX);
                    layoutParams.Y = initialY + (int)(e.RawY - initialTouchY);
                    windowManager.UpdateViewLayout(floatingView, layoutParams);
                    return true;
            }
            return false;
        }
    }
}