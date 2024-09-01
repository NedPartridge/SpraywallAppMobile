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
    List<Hold> HoldData;

    // SkiaPaint version of the wall image
    private SKBitmap wallImage;


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
        LoadBoundingBoxes();
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

    // Update visual control bindings
    private async Task UpdateWallBindings(int id)
    {
        wall = await GetWall(id);
        
        if(wall == null) // If someone's tampering with code behind, redirect them
        {
            await Shell.Current.GoToAsync("//" + nameof(SelectWall));
            return;
        } 
            

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

    private async void NavigateToHome(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//" + nameof(Home));
    }
    private async void NavigateToSettings(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//" + nameof(Settings));
    }
    private async void NavigateToLogbook(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//" + nameof(Logbook));
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




    // Climb management


    // Paint the holds over the image
    // Uses the SkiaSharp library
    // https://github.com/mono/SkiaSharp
    private void OnCanvasPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;

        // Clear the canvas
        canvas.Clear(SKColors.White);

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


    // Call this method after loading the JSON data
    private void LoadBoundingBoxes()
    {
        // Example to parse and add bounding boxes from JSON
        string jsonFilePath = Path.Combine(FileSystem.AppDataDirectory, "holds.json");
        string json = File.ReadAllText(jsonFilePath);
        HoldData = JsonSerializer.Deserialize<List<Hold>>(json);

        foreach (Hold box in HoldData)
        {
            boundingBoxes.Add(new SKRect(box.X1, box.Y1, box.X2, box.Y2));
        }

        Debug.WriteLine(HoldData.Count());
        // Invalidate the canvas to redraw with the bounding boxes
        WallCanvas.InvalidateSurface();
        Debug.WriteLine("Invalidate called");
    }
}