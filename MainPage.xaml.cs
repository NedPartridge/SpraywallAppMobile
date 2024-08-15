using System.Diagnostics;

namespace SpraywallAppMobile
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
        private void LoginButtonClicked(object sender, TappedEventArgs e)
        {
            Console.WriteLine("Login clicked!");
        }
    }

}
