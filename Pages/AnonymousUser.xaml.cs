namespace SpraywallAppMobile.Pages;


// Code behind for the 'Continue without signing in' page 
public partial class AnonymousUser : ContentPage
{
	public AnonymousUser()
	{
		InitializeComponent();
	}


    // Close the anonymity choice overlay
    private void OnCloseOverlayClicked(object sender, EventArgs e)
    {
        Overlay.IsVisible = false;
    }


    // Redirect user to the login page
    private async void OnLoginClick(object sender, EventArgs e)
    {
        Overlay.IsVisible = false;
        await Shell.Current.GoToAsync(nameof(LogIn));
    }

    // Open the choice to sign in to, or create, an account
    // TODO: prevent clicks 'behind' the overlay
    private void OnOpenOverlayClicked(object sender, TappedEventArgs e)
    {
        Overlay.IsVisible = true;
    }
}