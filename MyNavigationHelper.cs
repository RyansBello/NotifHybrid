using Microsoft.AspNetCore.Components;

public static class MyNavigationHelper
{
    public static NavigationManager? Navigation { get; set; }

    public static void Navigate(string uri)
    {
        if (Navigation is not null)
        {
            Navigation.NavigateTo(uri);
            System.Diagnostics.Debug.WriteLine($"✅ MyNavigationHelper Navigated to: {uri}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("⚠️ MyNavigationHelper NavigationManager belum tersedia");
        }
    }
}