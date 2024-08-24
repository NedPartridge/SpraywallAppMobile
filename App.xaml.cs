using SpraywallAppMobile.Models;

namespace SpraywallAppMobile;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Initialize settings and then set MainPage
        Task.Run(async () =>
        {
            await InitializeAppSettings();

            // Ensure AppShell doesn't depend on public key initialization
            MainPage = new AppShell();
        }).Wait();
    }

    // Initialize variables in AppSettings: Certain data (public keys, etc.) should be
    // updated every time the app starts
    private async Task InitializeAppSettings()
    {
        try
        {
            // Create a HttpClient to make web requests
            using (var httpClient = new HttpClient())
            {
                // Fetch and set the public key
                var response = await httpClient.GetStringAsync(AppSettings.RetrievePublicKeyAddress);
                AppSettings.PublicKeyXML = response;
                Console.WriteLine(response);
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., logging or user feedback)
            Console.WriteLine($"Error setting app settings: {ex}");
        }
    }
}
