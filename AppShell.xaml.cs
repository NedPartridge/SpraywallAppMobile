using SpraywallAppMobile.Pages;

namespace SpraywallAppMobile
{
    // The shell contains all pages, fields, etc
    public partial class AppShell : Shell
    {
        // Initialise the shell
        public AppShell()
        {
            InitializeComponent();

            // Map relevant pages to routes
            // Route names to be based on page class names
            // Routes allow the pages to be called up from other places in the application
            Routing.RegisterRoute(nameof(LogIn), typeof(LogIn));
            Routing.RegisterRoute(nameof(SignUp), typeof(SignUp));
            Routing.RegisterRoute(nameof(AnonymousUser), typeof(AnonymousUser));
            Routing.RegisterRoute(nameof(Home), typeof(Home));
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(SelectWall), typeof(SelectWall));
        }
    }
}
