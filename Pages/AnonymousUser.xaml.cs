namespace SpraywallAppMobile.Pages;


// Code behind for the 'Continue without signing in' page 
public partial class AnonymousUser : ContentPage
{
    // Initialise the component - only applicable to certain deployment platforms.
    public AnonymousUser()
	{
		InitializeComponent();
	}


    // Close the anonymity choice overlay
    private void OnCloseOverlayClicked(object sender, EventArgs e)
    {
        Overlay.IsVisible = false;
    }


    // Redirect user to the login/signup selection page 
    private async void OnLoginClick(object sender, EventArgs e)
    {
        Overlay.IsVisible = false;

        // Cannot route directly to page: instead, 'step' 'back' one page
        await Shell.Current.GoToAsync("..");
    }

    // Open the choice to sign in to, or create, an account
    private void OnOpenAccountOverlayClicked(object sender, EventArgs e)
    {
        Overlay.IsVisible = true;
    }

    // Redirect user to wall selection screen
    private async void OnSwitchWallClicked(object sender, EventArgs e)
    {
        Console.WriteLine("In progress, here");
    }

    // Increment the number of attempts on the currently visible climb
    private async void IncrementAttempts(object sender, EventArgs e)
    {
        Console.WriteLine("Attempts incremented");
    }

    // Decrement the number of attempts on the currently visible climb
    private async void DecrementAttempts(object sender, EventArgs e)
    {
        Console.WriteLine("Attempts decremented");
    }
}