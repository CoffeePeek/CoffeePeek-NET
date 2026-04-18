using System;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Avalonia.Android;

namespace CoffeePeek.Client.App.Android;

[Activity(
    Label = "CoffeePeek",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode,
    WindowSoftInputMode = SoftInput.AdjustResize)]
public class MainActivity : AvaloniaMainActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            Log.Error("APP", e.ExceptionObject.ToString());
        };
        base.OnCreate(savedInstanceState);

        // Resize layout when the keyboard opens (search and auth fields).
        Window?.SetSoftInputMode(SoftInput.AdjustResize);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
        {
            // Align with app palette (LightColors / stone surfaces).
            Window?.SetStatusBarColor(Color.ParseColor("#EDE0D3"));
            Window?.SetNavigationBarColor(Color.ParseColor("#FAFAF9"));
        }

    }
}