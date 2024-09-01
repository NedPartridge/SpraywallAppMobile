using SkiaSharp;
using SpraywallAppMobile.Models;
using SpraywallAppMobile.Helpers;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;
using System.Xml.Linq;

namespace SpraywallAppMobile.Pages;

// Page for setting new climbs
public partial class SetClimb : ContentPage
{
    // For web requests
    HttpClient client;

    // The current wall, and other available walls
    Wall wall;

    // The holds on this wall
    private List<SKRect> boundingBoxes = new List<SKRect>(); // for SkiaPaint functionality
    List<Hold> HoldData = new List<Hold>();

    // SkiaPaint version of the wall image
    private SKBitmap wallImage;

    Climb climb = new Climb();

    public SetClimb()
	{
		InitializeComponent();

        // Ignore SSL, set auth headers
        var httpClientHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        client = new HttpClient(httpClientHandler);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppSettings.Token);
    }


    // Initialise the page, every time the page is accessed.
    protected async override void OnAppearing()
    {
        base.OnAppearing();

        // Set user state
        HttpResponseMessage response = await client.GetAsync(AppSettings.absGetUserAddress);
        if (!response.IsSuccessStatusCode) // errors
        {   await DisplayAlert("Error", "Unknown", "go home");
            await Shell.Current.GoToAsync("//" + nameof(Home)); }
        // Success!
        string jsonResponse = await response.Content.ReadAsStringAsync();
        // Deserialize the JSON response to an object using System.Text.Json
        var user = JsonSerializer.Deserialize<RetrievedUser>(jsonResponse, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        climb = new() { Name = null, Grade = 0, SetterName = user.Name};


        // If state hasn't been set, go home
        if (StateHelper.CurrentWallId == null)
            await Shell.Current.GoToAsync("//" + nameof(Home));
        else // otherwise, use the provided id
            await UpdateWallBindings(Convert.ToInt32(StateHelper.CurrentWallId));

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

    // Update visual control bindings (not the image)
    private async Task UpdateWallBindings(int id)
    {
        wall = await GetWall(id);

        if (wall == null) // If someone's tampering with code behind, redirect them
        {
            await Shell.Current.GoToAsync("//" + nameof(SelectWall));
            return;
        }

        // wall data
        WallTitle.Text = wall.Name;
    }

    // Increment the number of attempts on the currently visible climb
    private async void IncrementAttempts(object sender, EventArgs e)
    {
        if (climb.Grade < 15) // max grade v15
            climb.Grade++;
        ClimbAttempts.Text = $"v{climb.Grade}"; // Bind to display
    }

    // Decrement the number of attempts on the currently visible climb
    private async void DecrementAttempts(object sender, EventArgs e)
    {
        if (climb.Grade > 0) // max grade v15
            climb.Grade--;
        ClimbAttempts.Text = $"v{climb.Grade}"; // Bind to display
    }

    // Discard all creation details, return to home
    private async void DiscardChanges(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//" + nameof(Home));
    }
    
    // Given the inputs (through UI), save a climb to the server
    private async void SaveClimb(object sender, EventArgs e)
    {
        // Validate it's a climb - no nulls, etc
        if(string.IsNullOrEmpty(ClimbName.Text))
        {
            await DisplayAlert("Invalid climb", "Name must not be null", "ok");
            return;
        }
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
                if(File.Exists(jsonFilePath))
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


    // Call this method after loading the JSON data
    private void LoadBoundingBoxes()
    {
        // Parse and add bounding boxes from JSON
        string jsonFilePath = Path.Combine(FileSystem.AppDataDirectory, "holds.json");
        string json = File.ReadAllText(jsonFilePath);
        HoldData.Clear();
        boundingBoxes.Clear();
        HoldData = JsonSerializer.Deserialize<List<Hold>>(json);

        foreach (Hold box in HoldData)
        {
            boundingBoxes.Add(new SKRect(box.X1, box.Y1, box.X2, box.Y2));
        }

        // Invalidate the canvas to redraw with the bounding boxes
        WallCanvas.InvalidateSurface();
    }
}