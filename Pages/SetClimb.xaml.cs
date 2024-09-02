using SkiaSharp;
using SpraywallAppMobile.Models;
using SpraywallAppMobile.Helpers;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using SkiaSharp.Views.Maui;
using System.Net.Http.Json;
using System.Text;

namespace SpraywallAppMobile.Pages;

// Page for setting new climbs
public partial class SetClimb : ContentPage
{
    // For web requests
    HttpClient client;

    // The current wall, and other available walls
    Wall wall;
    List<Hold> selectedHolds = new();

    int attempts = 1;

    // The holds on this wall
    private List<SkiaHold> boundingBoxes = new List<SkiaHold>(); // for SkiaPaint functionality
    List<Hold> HoldData = new List<Hold>();

    // SkiaPaint version of the wall image
    private SKBitmap wallImage;

    Climb climb = new Climb();

    float canvasWidth;
    float canvasHeight;

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
        climb = new() { Name = null, Grade = 0, SetterName = user.Name, Holds = new()};


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
        attempts++;
        ClimbAttempts.Text = attempts.ToString();
    }

    // Decrement the number of attempts on the currently visible climb
    private async void DecrementAttempts(object sender, EventArgs e)
    {
        if(attempts > 1)
            attempts--;
        ClimbAttempts.Text = attempts.ToString();
    }

    // Increment the climb's grade
    private async void IncrementGrade(object sender, EventArgs e)
    {
        if (climb.Grade < 15) // max grade v15
            climb.Grade++;
        ClimbGrade.Text = $"v{climb.Grade}"; // Bind to display
    }

    // Decrement the climb's grade
    private async void DecrementGrade(object sender, EventArgs e)
    {
        if (climb.Grade > 0) // min grade v0
            climb.Grade--;
        ClimbGrade.Text = $"v{climb.Grade}"; // Bind to display
    }


    // Discard all creation details, return to home
    private async void DiscardChanges(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//" + nameof(Home));
    }
    
    // Given the inputs (through UI), save a climb to the server
    private async void SaveClimb(object sender, EventArgs e)
    {
        // Validation is done server-side: no need to do so again here.
        ClimbToCreate climbTC = new ClimbToCreate();

        // Set climb properties from fields
        climbTC.Name = ClimbName.Text;
        climbTC.SetterName = climb.SetterName;
        climbTC.Attempts = attempts;
        climbTC.Grade = climb.Grade;
        climbTC.JsonHolds = JsonSerializer.Serialize(selectedHolds); // Serialise the holds
        // A climb must have holds
        if(string.IsNullOrEmpty(climbTC.JsonHolds) | climbTC.JsonHolds == "[]") 
        {
            await DisplayAlert("Must select holds", "obviously", "ok");
            return;
        }
        string jsonClimb = JsonSerializer.Serialize(climbTC); // Serialise the climb
        StringContent content = new(jsonClimb, Encoding.UTF8, "application/json"); // Prepare the climb to be transferred

        var response = await client.PostAsync((AppSettings.absCreateClimbAddress + $"/{wall.Id}"), content); // Make the request

        // Success :D
        if (response.IsSuccessStatusCode) 
        {
            await DisplayAlert("Success!", "", "ok");
            await Shell.Current.GoToAsync("//" + nameof(Home));
            return;
        }

        // Managed failed responses
        await DisplayAlert("Something's gone wrong", "Try again later", "ok");
        Debug.WriteLine(await response.Content.ReadAsStringAsync());
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
        canvasWidth = info.Width;
        canvasHeight = info.Height;

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

        // Paint for unselected bounding boxes
        var boxPaint = new SKPaint
        {
            Color = SKColors.Red,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 5
        };

        // Paint for selected bounding boxes
        var selectedBoxPaint = new SKPaint
        {
            Color = SKColors.Green,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 10
        };

        // Draw all bounding boxes
        foreach (var hold in boundingBoxes)
        {
            var scaledRect = new SKRect(
                offsetX + hold.SKRect.Left * scaleX,
                offsetY + hold.SKRect.Top * scaleY,
                offsetX + hold.SKRect.Right * scaleX,
                offsetY + hold.SKRect.Bottom * scaleY);

            // If the hold is selected, draw it in green
            if (climb.Holds.Contains(hold))
            {
                canvas.DrawRect(scaledRect, selectedBoxPaint);
            }
            else
            {
                canvas.DrawRect(scaledRect, boxPaint);
            }
        }
    }
    private void OnCanvasTouch(object sender, SKTouchEventArgs e)
    {
        // Only clicks, not drags
        if (!(e.ActionType == SKTouchAction.Pressed))
            return;

        // Calculate the aspect ratios
        float imageWidth = wallImage.Width;
        float imageHeight = wallImage.Height;
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

        // Get the touch location
        var touchX = e.Location.X;
        var touchY = e.Location.Y;
        
        // Scale touch - inverse of initial drawing operation
        var x = offsetX + touchX / scaleX;
        var y = (-offsetY + touchY) / scaleY;

        // Check if the touch is within any recognized bounding box
        var touchedHold = boundingBoxes.FirstOrDefault(hold =>
            hold.SKRect.Left <= x &&
            hold.SKRect.Top >= y &&
            hold.SKRect.Right >= x &&
            hold.SKRect.Bottom <= y);

        // If they hit a hold, either add it to the climb or remove it
        if (touchedHold != null)
        {
            // If the hold is not in the climb
            if (!climb.Holds.Contains(touchedHold)) 
            {
                // Add the hold to the climb
                climb.Holds.Add(touchedHold);
                selectedHolds.Add(new() 
                { 
                    X1=touchedHold.SKRect.Left, 
                    X2=touchedHold.SKRect.Right,
                    Y1 =touchedHold.SKRect.Bottom,
                    Y2=touchedHold.SKRect.Top
                });

                // Invalidate the surface to trigger a redraw
                WallCanvas.InvalidateSurface();
            }
            // Hold isn't in the climb - remove it
            else 
            {
                climb.Holds.Remove(touchedHold);
                selectedHolds.Remove(new()
                {
                    X1 = touchedHold.SKRect.Left,
                    X2 = touchedHold.SKRect.Right,
                    Y1 = touchedHold.SKRect.Bottom,
                    Y2 = touchedHold.SKRect.Top
                });

                // Invalidate the surface to trigger a redraw
                WallCanvas.InvalidateSurface();
            }
        }

        // All done, so why not exit nicely?
        e.Handled = true;
    }



    // After reading json data, load the 'boxes', which represent regions the holds may lie in
    private void LoadBoundingBoxes()
    {
        // Parse and add bounding boxes from JSON
        string jsonFilePath = Path.Combine(FileSystem.AppDataDirectory, "holds.json");
        string json = File.ReadAllText(jsonFilePath);
        HoldData.Clear();
        boundingBoxes.Clear();
        selectedHolds.Clear();
        HoldData = JsonSerializer.Deserialize<List<Hold>>(json);

        for(int i = 0; i < HoldData.Count(); i++)
        {
            Hold box = HoldData[i];

            // Add boxes to visual tree from data: 
            // x1 is left, x2 right, y1 bottom, y2 top
            // Skia is dumb why is the skrect like that?? this took me 20 minutes to figure out
            boundingBoxes.Add(new() { SKRect = new SKRect(box.X1, box.Y2, box.X2, box.Y1) });
        }
        // Invalidate the canvas to redraw with the bounding boxes
        WallCanvas.InvalidateSurface();
    }
}

// Hold data formatted so that skia can identify it
public class SkiaHold
{ 
    public SKRect SKRect { get; set; }
}