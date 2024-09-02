using SpraywallAppMobile.Helpers;
using SpraywallAppMobile.Models;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace SpraywallAppMobile.Pages;

// Code behind for the logbook, which lists all climbs the user has logged.
public partial class Logbook : ContentPage
{
    // Climbs in the logbook
    List<ClimbDto2> climbs = new();

    // For web requests
    HttpClient client;

    public Logbook()
    {
        InitializeComponent();
        var httpClientHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        client = new HttpClient(httpClientHandler);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppSettings.Token);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var response = await client.GetAsync(AppSettings.absGetUserClimbsAddress);

        if(!response.IsSuccessStatusCode)
        {
            await DisplayAlert("Error", "Maybe go log some climbs, and come back later?", "ok");
            await Shell.Current.GoToAsync("//" + nameof(Home));
            return;
        }


        climbs = JsonSerializer.Deserialize<List<ClimbDto2>>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if(climbs == null)
        {
            await DisplayAlert("No climbs logged", "", "Log some");
            await Shell.Current.GoToAsync("//" + nameof(Home));
            return;
        }

        WallsListView.ItemsSource = climbs;
    }

    // Go home
    private async void NavigateToHome(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//" + nameof(Home));
    }
    // Go to settings
    private async void NavigateToSettings(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//" + nameof(Settings));
    }
    // Go to logbook
    private async void NavigateToLogbook(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//" + nameof(Logbook));
    }

    // Redirect to the page of the tapped climb
    private async void OnClimbTapped(object sender, ItemTappedEventArgs e)
    {
        // Convert tapped event arguments into a helpful format (climbdto2) to get params
        if (e.Item is ClimbDto2 tappedClimb)
        {
            // Set current wall, redirect user to home
            StateHelper.CurrentWallId = tappedClimb.WallID;
            await Shell.Current.GoToAsync("//" + nameof(Home));
        }
    }
}