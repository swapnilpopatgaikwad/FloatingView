using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Timers;

namespace FloatingView
{
    [Service]
    public class FloatingButtonService : Service, View.IOnTouchListener
    {
        private WindowManagerLayoutParams layoutParams;
        private WindowManagerLayoutParams hiddenLayoutParams;
        private IWindowManager windowManager;
        private View floatingView;
        private View hiddenView;
        private float initialTouchX;
        private float initialTouchY;
        private int initialX;
        private int initialY;
        private bool isHidden;
        private Timer hideTimer;

        public override void OnCreate()
        {
            base.OnCreate();

            floatingView = LayoutInflater.From(this).Inflate(Resource.Layout.floating_button, null);
            hiddenView = LayoutInflater.From(this).Inflate(Resource.Layout.hidden_button, null);

            floatingView.SetOnTouchListener(this);
            hiddenView.Click += OnHiddenViewClick;

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

            hiddenLayoutParams = new WindowManagerLayoutParams(
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

            hideTimer = new Timer(10000) { AutoReset = false };
            hideTimer.Elapsed += (s, e) => RunOnUiThread(HideFloatingButton);
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
            if (hiddenView != null)
            {
                windowManager.RemoveView(hiddenView);
            }
        }

        private void OnFloatingButtonClick(object sender, EventArgs e)
        {
            Toast.MakeText(this, "Emergency Button Clicked", ToastLength.Short).Show();
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
                    hideTimer.Stop();
                    return true;

                case MotionEventActions.Up:
                    if (layoutParams.X < (windowManager.DefaultDisplay.Width / 2))
                    {
                        layoutParams.X = 0; 
                    }
                    else
                    {
                        layoutParams.X = windowManager.DefaultDisplay.Width - floatingView.Width;
                    }
                    windowManager.UpdateViewLayout(floatingView, layoutParams);
                    ShowHiddenView();
                    return true;

                case MotionEventActions.Move:
                    layoutParams.X = initialX + (int)(e.RawX - initialTouchX);
                    layoutParams.Y = initialY + (int)(e.RawY - initialTouchY);
                    windowManager.UpdateViewLayout(floatingView, layoutParams);
                    return true;
            }
            return false;
        }

        private void ShowHiddenView()
        {
            isHidden = true;
            hiddenLayoutParams.X = layoutParams.X < (windowManager.DefaultDisplay.Width / 2) ? 0 : windowManager.DefaultDisplay.Width - hiddenView.Width;
            hiddenLayoutParams.Y = layoutParams.Y;
            hiddenView.Background = layoutParams.X < (windowManager.DefaultDisplay.Width / 2) ? GetDrawable(Resource.Drawable.rounded_corners_left) : GetDrawable(Resource.Drawable.rounded_corners_right); // Updated here
            windowManager.RemoveView(floatingView);
            windowManager.AddView(hiddenView, hiddenLayoutParams);
        }

        private void OnHiddenViewClick(object sender, EventArgs e)
        {
            ShowFloatingButton();
            hideTimer.Start();
        }

        private void ShowFloatingButton()
        {
            isHidden = false;
            windowManager.RemoveView(hiddenView);
            windowManager.AddView(floatingView, layoutParams);
        }

        private void HideFloatingButton()
        {
            if (!isHidden)
            {
                ShowHiddenView();
            }
        }

        private void RunOnUiThread(Action action)
        {
            var handler = new Handler(Looper.MainLooper);
            handler.Post(action);
        }
    }
}