namespace SpraywallAppMobile.Pages;


// Code behind class for the login page
public partial class LogIn : ContentPage
{
    // Initialise the component - only applicable to certain deployment platforms.
	public LogIn()
	{
		InitializeComponent();
	}

    // Exit the login screen, go back to the signup/login choice 
    // Do NOT set the currently logged in user.
    private async void OnBackButtonClicked(object sender, TappedEventArgs e)
    {
        // Cannot route directly to page: instead, 'step' 'back' one page
        await Shell.Current.GoToAsync("..");
    }

    // Login the user, by checking provided details against API.
    private async void OnLoginButtonClicked(object sender, TappedEventArgs e)
    {
        // TODO: Authenticate the user
        // Encrypt 'password' text, send post reqq to API
        // Blocked by: No api yet :/
    }

}