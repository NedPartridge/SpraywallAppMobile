using SpraywallAppMobile.Helpers;
using SpraywallAppMobile.Models;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SpraywallAppMobile.Pages;

public partial class Home : ContentPage
{
    HttpClient client;
    Wall wall;
    List<WallDTO> walls = new List<WallDTO>();

	public Home()
	{
		InitializeComponent();
        // Ignore SSL
        var httpClientHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        client = new HttpClient(httpClientHandler);
    }

    // Initialise the page, every time the page is accessed.
    protected async override void OnAppearing()
    {
        base.OnAppearing();

        // Update wall list from server
        await GetSavedWalls();

        // If wall hasn't been set (ie, there is no wall selected), set it to the first item in the walls list
        // Then, bind controls to the downloaded data
        var wallToQuery = walls.FirstOrDefault();
        if (wallToQuery == null) // If the user hasn't added any walls, direct them to do so
        {
            await Shell.Current.GoToAsync("//" + nameof(SelectWall));
            return;
        }
        if (StateHelper.CurrentWallId == null) // If state hasn't been set, default.
            await UpdateWallBindings(wallToQuery.Id);
        else // otherwise, use the provided id
            await UpdateWallBindings(StateHelper.CurrentWallId);

        // Walls select content
        WallsListView.ItemsSource = walls;
    }

    // Update visual control bindings
    private async Task UpdateWallBindings(int id)
    {
        wall = await GetWall(id);
        WallImage.Source = ImageSource.FromFile(wall.ImagePath);
        WallTitle.Text = wall.Name;
    }

    // Redirect user to the login/signup selection page 
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//" + nameof(MainPage));
    }

    // Redirect user to wall selection screen
    private async void OnSwitchWallClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//" + nameof(SelectWall));
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

    private async void OnOpenWallSelectOverlayClicked(object sender, EventArgs e)
    {
        WallSelectOverlay.IsVisible = true;
    }
    private async void OnCloseWallSelectOverlayClicked(object sender, EventArgs e)
    {
        WallSelectOverlay.IsVisible = false;
    }

    // Change the wall the home page is displaying
    private async void OnWallItemTapped(object sender, ItemTappedEventArgs e)
    {
        // Convert tapped event arguments into a helpful format (walldto)
        if (e.Item is WallDTO tappedWall)
        {
            await UpdateWallBindings(tappedWall.Id);
            StateHelper.CurrentWallId = tappedWall.Id;
        }
        // Disable popup
        WallSelectOverlay.IsVisible = false;
    }

    // Retrieve a wall from the webserver, and store it's data
    private async Task<Wall?> GetWall(int id)
    {
        HttpResponseMessage response = await client.GetAsync(AppSettings.absGetWallAddress + $"/{id}");

        // Check if the request was successful
        if (response.IsSuccessStatusCode)
        {
            // Read the response content as a stream
            var responseStream = await response.Content.ReadAsStreamAsync();

            // Deserialize the response into a Wall object
            // Save the wall and json data locally, rather than holding in memory.
            using (var document = JsonDocument.Parse(responseStream))
            {
                var root = document.RootElement;

                // Get wall ID and Name
                int wallIdFromResponse = root.GetProperty("id").GetInt32();
                string wallName = root.GetProperty("name").GetString();

                // Download the Image - stored at the one address, more than one wall image
                // is never needed, and hence, should never be stored.
                string imageBase64 = root.GetProperty("image").GetString();
                byte[] imageBytes = Convert.FromBase64String(imageBase64); 
                string imagePath = Path.Combine(FileSystem.AppDataDirectory, "wall_image.webp");
                await File.WriteAllBytesAsync(imagePath, imageBytes);

                // Read and save the JSON file content
                string jsonFileContent = root.GetProperty("jsonFile").GetString();
                string jsonFilePath = Path.Combine(FileSystem.AppDataDirectory, "holds.json");
                await File.WriteAllTextAsync(jsonFilePath, jsonFileContent);

                return new Wall()
                {
                    Id = wallIdFromResponse,
                    Name = wallName,
                    ImagePath = imagePath,
                    JsonPath = jsonFilePath
                };
            }
        }
        else // Manage failed requests
        {
            if (await response.Content.ReadAsStringAsync() == "Invalid credentials")
                await DisplayAlert("Invalid request", "Try logging out/in?", "ok");
            else
                await DisplayAlert("Something went wrong", "We don't know what, please try again later", "ok");
            return null;
        }
    }

    // Retrieve the list of walls the user has accessed
    private async Task GetSavedWalls()
    {
        try
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppSettings.Token);
            HttpResponseMessage response = await client.GetAsync(AppSettings.absGetSavedWallsAddress);
            string jsonWalls = await response.Content.ReadAsStringAsync();
            Debug.WriteLine(jsonWalls);
            // Deserialise the list of walls, for use in future requests
            walls = JsonSerializer.Deserialize<List<WallDTO>>(jsonWalls);
            Debug.WriteLine("Wallcount: " + walls.Count());
        }
        catch (Exception ex) { await DisplayAlert("Error", "Check connection", "ok"); }
    }
}