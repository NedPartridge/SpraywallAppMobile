namespace SpraywallAppMobile.Pages;

public partial class LogIn : ContentPage
{
	public LogIn()
	{
		InitializeComponent();
	}

    // Exit the login screen, go back to the signup/login choice
    private async void OnBackButtonClicked(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(MainPage));
    }

    // Login the user
    private async void AnonymousUserButtonClicked(object sender, TappedEventArgs e)
    {
        // TODO: Authenticate the user
        // Encrypt 'password' text, send post reqq to API
        // Blocked by: No api yet :/
    }
}