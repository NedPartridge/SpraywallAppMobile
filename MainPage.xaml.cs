using System.Diagnostics;

namespace SpraywallAppMobile
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
        private async void LogInButtonClicked(object sender, TappedEventArgs e)
        {
            await Shell.Current.GoToAsync("login");
        }
        private async void SignUpButtonClicked(object sender, TappedEventArgs e)
        {
            await Shell.Current.GoToAsync("signup");
        }

        private async void AnonymousUserButtonClicked(object sender, TappedEventArgs e)
        {
            await Shell.Current.GoToAsync("anonymoususer");
        }
    }

}
