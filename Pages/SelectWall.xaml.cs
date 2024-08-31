using SpraywallAppMobile.Helpers;
using SpraywallAppMobile.Models;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace SpraywallAppMobile.Pages;

public partial class SelectWall : ContentPage
{
    HttpClient client;
	public SelectWall()
	{
		InitializeComponent();
        var httpClientHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        client = new HttpClient(httpClientHandler);
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
        await Shell.Current.GoToAsync("//" + nameof(Home));
    }

    // Try to save the wall with the given ID
    private async void OnSaveWallFromIdClicked(object sender, EventArgs e)
    {
        int wallId;
        try 
        {
            if (WallId.Text == null | WallId.Text == "") // existence check
                throw new Exception();
            wallId = Convert.ToInt32(WallId.Text); // Type check
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppSettings.Token);
            HttpResponseMessage wall = await client.GetAsync(AppSettings.absSaveWallAddress + $"/{wallId}");
            if (!wall.IsSuccessStatusCode) // range check
                throw new Exception();
            StateHelper.CurrentWallId = wallId;
            await Shell.Current.GoToAsync("//" + nameof(Home));
        }
        catch // Alert the user
        {
            await DisplayAlert("Enter a valid wall id", "dumbass", "ok");
            return;
        }
    }
}