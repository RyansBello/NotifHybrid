using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Plugin.Firebase.Auth;
using Plugin.Firebase.Bundled.Shared;
using System.Globalization;

#if ANDROID
using Plugin.Firebase.Crashlytics;
using Plugin.Firebase.Bundled.Platforms.Android;
using Plugin.Firebase.CloudMessaging;
#endif

namespace NotifHybrid
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .RegisterFirebaseServices()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }    

    private static MauiAppBuilder RegisterFirebaseServices(this MauiAppBuilder builder)
        {
            builder.ConfigureLifecycleEvents(events => {
#if ANDROID
                events.AddAndroid(android => android.OnCreate((activity, _) =>
                CrossFirebase.Initialize(activity, CreateCrossFirebaseSettings())));
                CrossFirebaseCloudMessaging.Current.NotificationTapped += (s, e) =>
                {
                    System.Diagnostics.Debug.WriteLine("🔥 NotificationTapped triggered");

                    if (e.Notification.Data.TryGetValue("targetPage", out var targetPage))
                    {
                        System.Diagnostics.Debug.WriteLine($"➡️ Navigasi ke: {targetPage}");
                        Preferences.Set("PendingNavigation", targetPage);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("⚠️ targetPage tidak ditemukan dalam data notifikasi");
                    }
                };

                events.AddAndroid(android => android
                    .OnResume(activity =>
                    {
                        var targetPage = Preferences.Get("PendingNavigation", null);
                        if (!string.IsNullOrEmpty(targetPage))
                        {
                            Preferences.Remove("PendingNavigation");

                            System.Timers.Timer retryTimer = new System.Timers.Timer(500); // setiap 500 ms
                            retryTimer.Elapsed += (sender, args) =>
                            {
                                if (MyNavigationHelper.Navigation is not null)
                                {
                                    retryTimer.Stop();
                                    retryTimer.Dispose();

                                    MainThread.BeginInvokeOnMainThread(() =>
                                    {
                                        MyNavigationHelper.Navigate(targetPage);
                                    });
                                }
                            };
                            retryTimer.Start();
                        }
                    })
                );

                CrossFirebaseCrashlytics.Current.SetCrashlyticsCollectionEnabled(true);
#endif
            });

            builder.Services.AddSingleton(_ => CrossFirebaseAuth.Current);

            // Setting default culture info to "en-US" to ensure colon (:) is used
            var culture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            // Set timezone to WIT (Waktu Indonesia Timur)
            TimeZoneInfo indonesiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"); // for WIT

            // Convert DateTime to local time on app start
            DateTime localTime = TimeZoneInfo.ConvertTime(DateTime.Now, indonesiaTimeZone);


            return builder;
        }
        private static CrossFirebaseSettings CreateCrossFirebaseSettings()
        {
            return new CrossFirebaseSettings(
                isAuthEnabled: true,
                isCloudMessagingEnabled: true,
                isAnalyticsEnabled: true);
        }
    }
}