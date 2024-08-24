using SpraywallAppMobile.Pages;
using System.Diagnostics;

namespace SpraywallAppMobile;

// Code behind for the landing page
public partial class MainPage : ContentPage
{
    // Initialise the component - only applicable to certain deployment platforms.
    public MainPage()
    {
        InitializeComponent();
    }
        
    // Send user to account log in page
    private async void LogInButtonClicked(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(LogIn));
    }

    // Send user to new account creation page
    private async void SignUpButtonClicked(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SignUp));
    }

    // 'Continue without signing in'
    // Access a page with limited functionality - no logging, no saving data, etc
    private async void AnonymousUserButtonClicked(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AnonymousSelectWall));
    }
}