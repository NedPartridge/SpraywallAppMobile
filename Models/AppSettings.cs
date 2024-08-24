namespace SpraywallAppMobile.Models
{
    // A collection of non-sensitive application data: URLs, public keys, etc.
    static class AppSettings
    {
        // Since the server is locally deployed, the 'base' URL is different between platforms
        public static readonly Uri BaseUrl;

        static AppSettings()
        {
            try
            {
                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    // Use 10.0.2.2 for Android emulators
                    BaseUrl = new Uri("https://10.0.2.2:7167/");
                }
                else if (DeviceInfo.Platform == DevicePlatform.iOS || DeviceInfo.Platform == DevicePlatform.macOS)
                {
                    // Use localhost for iOS and macOS as they should refer to the development machine
                    BaseUrl = new Uri("https://localhost:7167/");
                }
                else if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    // Use localhost for Windows as well
                    BaseUrl = new Uri("https://localhost:7167/");
                }
                else
                {
                    throw new PlatformNotSupportedException("Platform not supported");
                }

                // Ensure BaseUrl is not null
                if (BaseUrl == null)
                {
                    throw new InvalidOperationException("BaseUrl could not be initialized.");
                }

                // API endpoint URLs
                RetrievePublicKeyAddress = new Uri(BaseUrl, "User/publickey");
                LogInAddress = new Uri(BaseUrl, "User/login");
                SignUpAddress = new Uri(BaseUrl, "User/signup");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing AppSettings: {ex}");
                throw;
            }
        }

        // API endpoint URLs
        public static readonly Uri RetrievePublicKeyAddress;
        public static readonly Uri LogInAddress;
        public static readonly Uri SignUpAddress;

        // The public key used to encrypt password data
        // Set by the sign up/in pages, on init
        public static string PublicKeyXML { get; set; } = "";

        // The security token to append to 'sensitive' requests
        public static string Token { get; set; } = "";
    }
}
