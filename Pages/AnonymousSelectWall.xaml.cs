namespace SpraywallAppMobile.Pages;

public partial class AnonymousSelectWall : ContentPage
{
	public AnonymousSelectWall()
	{
		InitializeComponent();
	}

    // Open the choice to sign in to, or create, an account
    private void OnOpenAccountOverlayClicked(object sender, EventArgs e)
    {
        Overlay.IsVisible = true;
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

        await Shell.Current.GoToAsync("//" + nameof(MainPage));
    }



    // Open the search overlay
    private void OnOpenSearchClicked(object sender, EventArgs e)
    {
        Scanner.IsVisible = false;
        Search.IsVisible = true;
    }

    // Open the scan overlay
    private void OnOpenScanClicked(object sender, EventArgs e)
    {
        Scanner.IsVisible = true;
        Search.IsVisible = false;
    }




    // Called when user closes wall selection without making a choice
    private async void OnDiscardChoiceClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//" + nameof(AnonymousUser));
    }

    // Send user back to anonymous home, after updating the wall selection
    private async void OnSubmitWallChoiceClicked(object sender, EventArgs e)
    {
        // TODO: update wall
        await Shell.Current.GoToAsync("//" + nameof(AnonymousUser));
    }
}