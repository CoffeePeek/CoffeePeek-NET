using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using Avalonia.Android;

namespace CoffeePeek.Client.App.Android;

[Activity(
    Label = "CoffeePeek.Client.App.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            Log.Error("APP", e.ExceptionObject.ToString());
        };
        base.OnCreate(savedInstanceState);
    }
}