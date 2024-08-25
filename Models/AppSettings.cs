namespace SpraywallAppMobile.Models
{
    // A collection of non-sensitive application data: URLs, public keys, etc.
    static class AppSettings
    {
        // Since the server is locally deployed, the 'base' URL is different between platforms
        public static readonly string AndroidBaseUrl = new("https://10.0.2.2:7167/");
        public static readonly string DefaultBaseUrl = new("https://localhost:7167/");

        // API endpoint URLs
        public static readonly string RetrievePublicKeyAddress = "User/publickey";
        public static readonly string LogInAddress = "User/login";
        public static readonly string SignUpAddress = "User/signup";

        // Absolute URLs - initialised in app.xaml.cs, under the constructor
        public static Uri absRetrievePublicKeyAddress { get; set; }
        public static Uri absLogInAddress { get; set; }
        public static Uri absSignUpAddress { get; set; }


        // The public key used to encrypt password data
        // Set by the sign up/in pages, on init
        public static string PublicKeyXML { get; set; } = "";

        // The security token to append to 'sensitive' requests
        public static string Token { get; set; } = "";
    }
}
