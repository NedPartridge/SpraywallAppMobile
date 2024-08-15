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
            // Routes to all be totally lowercase
            // Routes allow the pages to be called up from other places in the application
            Routing.RegisterRoute("login", typeof(LogIn));
            Routing.RegisterRoute("signup", typeof(SignUp));
            Routing.RegisterRoute("anonymoususer", typeof(AnonymousUser));
        }
    }
}
