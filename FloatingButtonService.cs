using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Timers;
using Handler = Android.OS.Handler;

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
        private Button closeButton;
        private Button emergencyButton;
        private float initialTouchX;
        private float initialTouchY;
        private int initialX;
        private int initialY;
        private bool isHidden;
        private bool isFloatingViewAdded;
        private bool isHiddenViewAdded;
        private Timer hideTimer;
        private int screenHeight;

        private Handler handler = new Handler();
        private const int CLICK_DELAY = 1000;

        public override void OnCreate()
        {
            base.OnCreate();

            floatingView = LayoutInflater.From(this).Inflate(Resource.Layout.floating_button, null);
            hiddenView = LayoutInflater.From(this).Inflate(Resource.Layout.hidden_button, null);

            emergencyButton = floatingView.FindViewById<Button>(Resource.Id.floating_button);
            closeButton = floatingView.FindViewById<Button>(Resource.Id.close_button);

            floatingView.SetOnTouchListener(this);
            closeButton.Click += OnCloseButtonClick;
            emergencyButton.Click += OnEmergencyButtonClick;
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
            isFloatingViewAdded = true;

            screenHeight = windowManager.DefaultDisplay.Height;

            hideTimer = new Timer(10000) { AutoReset = false };
            hideTimer.Elapsed += (s, e) => RunOnUiThread(HideFloatingButton);

            StartForegroundService();
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (isFloatingViewAdded)
            {
                windowManager.RemoveView(floatingView);
                isFloatingViewAdded = false;
            }
            if (isHiddenViewAdded)
            {
                windowManager.RemoveView(hiddenView);
                isHiddenViewAdded = false;
            }
        }

        private void OnCloseButtonClick(object sender, EventArgs e)
        {
            StopSelf();
        }

        private void ShowToast(string message)
        {
            RunOnUiThread(() =>
            {
                handler.PostDelayed(() =>
                {
                    Toast.MakeText(this, message, ToastLength.Short).Show();

                }, CLICK_DELAY);
            });
        }

        private void OnEmergencyButtonClick(object sender, EventArgs e)
        {
            ShowToast("Emergency Button Clicked");
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
                    if (layoutParams.Y >= screenHeight - floatingView.Height * 2)
                    {
                        closeButton.Visibility = ViewStates.Visible;
                        emergencyButton.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        closeButton.Visibility = ViewStates.Gone;
                        emergencyButton.Visibility = ViewStates.Visible;

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
                    }
                    return true;

                case MotionEventActions.Move:
                    layoutParams.X = initialX + (int)(e.RawX - initialTouchX);
                    layoutParams.Y = initialY + (int)(e.RawY - initialTouchY);
                    windowManager.UpdateViewLayout(floatingView, layoutParams);

                    if (layoutParams.Y >= screenHeight - floatingView.Height * 2)
                    {
                        closeButton.Visibility = ViewStates.Visible;
                        emergencyButton.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        closeButton.Visibility = ViewStates.Gone;
                        emergencyButton.Visibility = ViewStates.Visible;
                    }
                    return true;
            }
            return false;
        }

        private void ShowHiddenView()
        {
            if (isFloatingViewAdded)
            {
                windowManager.RemoveView(floatingView);
                isFloatingViewAdded = false;
            }

            if (isHiddenViewAdded)
            {
                windowManager.RemoveView(hiddenView);
                isHiddenViewAdded = false;
            }

            isHidden = true;
            hiddenLayoutParams.X = layoutParams.X < (windowManager.DefaultDisplay.Width / 2) ? 0 : windowManager.DefaultDisplay.Width - hiddenView.Width;
            hiddenLayoutParams.Y = layoutParams.Y;
            hiddenView.Background = layoutParams.X < (windowManager.DefaultDisplay.Width / 2) ? GetDrawable(Resource.Drawable.rounded_corners_left) : GetDrawable(Resource.Drawable.rounded_corners_right);
            windowManager.AddView(hiddenView, hiddenLayoutParams);
            isHiddenViewAdded = true;
        }

        private void OnHiddenViewClick(object sender, EventArgs e)
        {
            ShowFloatingButton();
            hideTimer.Start();
        }

        private void ShowFloatingButton()
        {
            if (isHiddenViewAdded)
            {
                windowManager.RemoveView(hiddenView);
                isHiddenViewAdded = false;
            }

            if (!isFloatingViewAdded)
            {
                windowManager.AddView(floatingView, layoutParams);
                isFloatingViewAdded = true;
            }

            isHidden = false;
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

        private void StartForegroundService()
        {
            var channelId = "floating_button_channel";
            var channelName = "Floating Button Service";

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var notificationChannel = new NotificationChannel(channelId, channelName, NotificationImportance.Low);
                var notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager.CreateNotificationChannel(notificationChannel);
            }

            var notification = new Notification.Builder(this, channelId)
                .SetContentTitle("Floating Button Service")
                .SetContentText("Emergency button is active.")
                .SetSmallIcon(Resource.Mipmap.ic_launcher_round)
                .SetOngoing(true)
                .Build();

            StartForeground(1, notification);
        }
    }
}