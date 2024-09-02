using SpraywallAppMobile.Helpers;
using SpraywallAppMobile.Models;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace SpraywallAppMobile.Pages;

public partial class AnonymousSelectWall : ContentPage
{
    HttpClient client;
    public AnonymousSelectWall()
	{
		InitializeComponent();
        var httpClientHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        client = new HttpClient(httpClientHandler);
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
    // Called when user closes wall selection without making a choice
    private async void OnDiscardChoiceClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//" + nameof(Home));
    }

    // Confirm the wall with the given id exists. if so, nav there
    private async void OnSubmitWallChoiceClicked(object sender, EventArgs e)
    {
        int wallId;
        try
        {
            if (WallId.Text == null | WallId.Text == "") // existence check
                throw new Exception();
            wallId = Convert.ToInt32(WallId.Text); // Type check
            HttpResponseMessage wall = await client.GetAsync(AppSettings.absIsWallAddress + $"/{wallId}");
            if (!wall.IsSuccessStatusCode) // range check - is it a valid wall?
                throw new Exception();
            StateHelper.CurrentWallId = wallId;
            await Shell.Current.GoToAsync("//" + nameof(AnonymousUser));
        }
        catch // Alert the user
        {
            await DisplayAlert("Enter a valid wall id", "dumbass", "ok");
            return;
        }
    }
}