using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Plugin.Firebase.CloudMessaging;

namespace NotifHybrid
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        public static MainActivity? Instance { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HandleIntent(Intent!);
            CreateNotificationChannelIfNeeded();
            // Then, make the assignment in the OnCreate method.
            Instance = this;
        }

        private void HandleIntent(Intent intent)
        {
            FirebaseCloudMessagingImplementation.OnNewIntent(intent);
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            HandleIntent(intent);
        }

        private void CreateNotificationChannelIfNeeded()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                CreateNotificationChannel();
            }
        }

        private void CreateNotificationChannel()
        {
            var channelId = $"{PackageName}.general";
            var notificationManager = (NotificationManager)GetSystemService(NotificationService)!;
            var channel = new NotificationChannel(channelId, "General", NotificationImportance.Default);
            notificationManager.CreateNotificationChannel(channel);
            FirebaseCloudMessagingImplementation.ChannelId = channelId;
            //FirebaseCloudMessagingImplementation.SmallIconRef = Resource.Drawable.ic_push_small;
        }

        public override bool DispatchKeyEvent(KeyEvent e)
        {
            // Workaround: The back button will automatically navigate back if possible, it is not
            // possible to intercept it in MainPage.OnBackButtonPressed().
            if ((e.KeyCode == Keycode.Back) && (e.Action == KeyEventActions.Down))
            {
                return false;
            }

            return base.DispatchKeyEvent(e);
        }
    }
}
