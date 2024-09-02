using SpraywallAppMobile.Helpers;
using SpraywallAppMobile.Models;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;


namespace SpraywallAppMobile.Pages;

public partial class Home : ContentPage
{
    // For web requests
    HttpClient client;

    // The current wall, and other available walls
    Wall wall;
    List<WallDTO> walls = new List<WallDTO>();

    // The holds on this wall
    private List<SKRect> boundingBoxes = new List<SKRect>(); // for SkiaPaint functionality
    List<Hold> HoldData = new List<Hold>();

    // SkiaPaint version of the wall image
    private SKBitmap wallImage;

    // The available, and currently displayed, climb ids
    List<int> climbIds = new List<int>();
    int climbId;
    int climbIndex = 0;

    bool logged; // whether the climb has been logged

    ClimbDto climb;
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
            await UpdateWallBindings(Convert.ToInt32(StateHelper.CurrentWallId));

        // Walls select content
        WallsListView.ItemsSource = walls;

        // Load the wall image for skia
        LoadWallImage();
        // Load the holds for the current wall
        await LoadBoundingBoxes();
    }
    private void LoadWallImage()
    {
        // Assuming you have the path to the image file
        var imagePath = Path.Combine(FileSystem.AppDataDirectory, "wall_image.webp");

        // Load the image into an SKBitmap
        using (var stream = File.OpenRead(imagePath))
        {
            wallImage = SKBitmap.Decode(stream);
        }
    }

    // Update visual control bindings (not the image, that's skiasharp)
    private async Task UpdateWallBindings(int id)
    {
        climbIndex = 0;
        wall = await GetWall(id);

        if (wall == null) // If someone's tampering with code behind, redirect them
        {
            await Shell.Current.GoToAsync("//" + nameof(SelectWall));
            return;
        }
        // Set wall bindings
        var response = await client.GetAsync(AppSettings.absGetClimbsAddress + $"/{id}");
        climbIds = JsonSerializer.Deserialize<List<int>>(await response.Content.ReadAsStringAsync());
        StateHelper.CurrentWallId = id;

        // Set climb bindings: redirect if no climbs
        if (climbIds == null | climbIds.Count() == 0)
        {
            await DisplayAlert("This walls has no climbs", "Add some", "ok");
            await Shell.Current.GoToAsync("//" + nameof(SetClimb));
            return;
        }
        climbId = climbIds[climbIndex];
        // wall data
        WallTitle.Text = wall.Name;
    }

    // Quit the app
    // Maui has no easy way to erase page data, because it's crap
    // Best solution? restart the app
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Goodbye!", "", "bye?");
        Application.Current.Quit();
    }

    // Redirect user to wall selection screen
    private async void OnSwitchWallClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//" + nameof(SelectWall));
    }

    // Overlay management
    private async void OnOpenWallSelectOverlayClicked(object sender, EventArgs e)
    {
        WallSelectOverlay.IsVisible = true;
    }
    private async void OnCloseWallSelectOverlayClicked(object sender, EventArgs e)
    {
        WallSelectOverlay.IsVisible = false;
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

    // Go to set climb
    private async void OnSetClimbClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//" + nameof(SetClimb));
    }

    // Log a climb
    private async void OnLogClimbClicked(object sender, EventArgs e)
    {
        if(logged)
        {
            await DisplayAlert("Climb already logged", "", "ok");
            return;
        }
        var r = await client.GetAsync(AppSettings.absLogClimbAddress + $"/{climb.Id}/{climb.Attempts}");
        await LoadBoundingBoxes();
    }


    // Increment the current climb
    private async void NextClimbId(object sender, EventArgs e)
    {
        climbIndex++;
        if (climbIndex == climbIds.Count()) // If at the end of the list, reset to 0
            climbIndex = 0;
        await LoadBoundingBoxes();
    }
    // Decrement the current climb
    private async void LastClimbId(object sender, EventArgs e)
    {
        climbIndex--;
        if (climbIndex == -1) // If at the start of the list, reset to the end
            climbIndex = climbIds.Count()-1;
        await LoadBoundingBoxes();
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
        // Load the wall image for skia
        LoadWallImage();
        // Load the holds for the current wall
        await LoadBoundingBoxes();

        // Disable popup
        WallSelectOverlay.IsVisible = false;
    }

    private void IncrementAttempts(object sender, EventArgs e)
    {
        climb.Attempts++;
        ClimbAttempts.Text = climb.Attempts.ToString();
    }

    private void DecrementAttempts(object sender, EventArgs e)
    {
        if (climb.Attempts == 1)
            return;
        climb.Attempts--;
        ClimbAttempts.Text = climb.Attempts.ToString();
    }
    private async void OnFlagClimbClicked(object sender, EventArgs e)
    {
        var response = await client.GetAsync(AppSettings.absFlagClimbAddress + $"/{climb.Id}");

        if (!response.IsSuccessStatusCode)
            await DisplayAlert("Something went wrong", "Try again later", "Ok");
        else
            await DisplayAlert("Climb flagged", "", "ok");
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
                if (File.Exists(jsonFilePath))
                    File.Delete(jsonFilePath);
                await File.WriteAllTextAsync(jsonFilePath, jsonFileContent);

                return new Wall() // Return 'small' data - no need to load images/json into memory here
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
            // Deserialise the list of walls, for use in future requests
            walls = JsonSerializer.Deserialize<List<WallDTO>>(jsonWalls);
        }
        catch (Exception ex) { await DisplayAlert("Error", "Check connection", "ok"); }
    }

    // Climb management


    // Paint the holds over the image
    // Uses the SkiaSharp library
    // https://github.com/mono/SkiaSharp
    private void OnCanvasPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;

        // Clear the canvas
        canvas.Clear();

        if (wallImage == null || boundingBoxes == null || boundingBoxes.Count == 0)
            return;

        // Calculate the aspect ratios
        float imageWidth = wallImage.Width;
        float imageHeight = wallImage.Height;
        float canvasWidth = info.Width;
        float canvasHeight = info.Height;

        float imageAspectRatio = imageWidth / imageHeight;
        float canvasAspectRatio = canvasWidth / canvasHeight;

        // Calculate the scaling factors
        float scaleX, scaleY;
        if (imageAspectRatio > canvasAspectRatio)
        {
            // Image is wider than the canvas
            scaleX = canvasWidth / imageWidth;
            scaleY = scaleX; // Maintain aspect ratio
        }
        else
        {
            // Image is taller or equal aspect ratio to canvas
            scaleY = canvasHeight / imageHeight;
            scaleX = scaleY; // Maintain aspect ratio
        }

        // Calculate the offset to center the image
        float offsetX = (canvasWidth - imageWidth * scaleX) / 2;
        float offsetY = (canvasHeight - imageHeight * scaleY) / 2;

        // Draw the wall image
        using (var paint = new SKPaint())
        {
            canvas.DrawBitmap(wallImage, new SKRect(offsetX, offsetY, offsetX + imageWidth * scaleX, offsetY + imageHeight * scaleY), paint);
        }

        // Define the paint for the bounding boxes
        var boxPaint = new SKPaint
        {
            Color = SKColors.Red,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 5
        };

        // Draw each bounding box
        foreach (var box in boundingBoxes)
        {
            var scaledRect = new SKRect(
                offsetX + box.Left * scaleX,
                offsetY + box.Top * scaleY,
                offsetX + box.Right * scaleX,
                offsetY + box.Bottom * scaleY);

            canvas.DrawRect(scaledRect, boxPaint);
        }
    }


    // Reload the climb data
    private async Task LoadBoundingBoxes()
    {
        // Clear data from previous gets/drawings
        HoldData.Clear();
        boundingBoxes.Clear();

        climbId = climbIds[climbIndex];

        // Retrieve the current climb
        var response = await client.GetAsync(AppSettings.absGetClimbAddress + $"/{climbId}");
        if (response.IsSuccessStatusCode)
        {
            climb = JsonSerializer.Deserialize<ClimbDto>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            HoldData = JsonSerializer.Deserialize<List<Hold>>(climb.JsonHolds);
            ClimbName.Text = climb.Name;
            ClimbGrade.Text = $"v{climb.Grade}";
            ClimbSetter.Text = climb.SetterName;

            if(climb.Attempts == null)
            {
                logged = false;
                climb.Attempts = 1;
                // Enable climb logging
                ClimbAttempts.Text = climb.Attempts.ToString();
                AttemptsStatic.IsVisible = false;
                AttemptsWrapper.IsVisible = true;
                LogButton.IsVisible = true;
            }
            else
            {
                logged = true;
                // Disable climb logging
                // Just show attempts
                AttemptsStatic.IsVisible = true;
                AttemptsStatic.Text = "Attempts: " + climb.Attempts.ToString();
                AttemptsWrapper.IsVisible = false;
                LogButton.IsVisible = false;
            }
        }
        else
        {
            await DisplayAlert("Something's wrong", "Come back later", "ok");
            Application.Current.Quit();
            return; 
        }

        // Make each hold in the climb available to draw
        foreach (Hold box in HoldData)
        {
            boundingBoxes.Add(new SKRect(box.X1, box.Y1, box.X2, box.Y2));
        }

        // Invalidate the canvas to redraw with the bounding boxes
        WallCanvas.InvalidateSurface();
    }
}