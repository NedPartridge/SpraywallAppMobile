using SpraywallAppMobile.Models;
using System.Text;

namespace SpraywallAppMobile.Pages;

public partial class SignUp : ContentPage
{
    
	public SignUp()
	{
		InitializeComponent();
	}

    // Exit the sign up screen, go back to the signup/login choice
    private async void OnBackButtonClicked(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(MainPage));
    }

    // Create a new user account, based on the details provided
    // But first, authenticate the deetz.
    private async void AnonymousUserButtonClicked(object sender, TappedEventArgs e)
    {
        if(!IsValidEmail(email.Text))
        {
            await DisplayAlert("Invalid email", "Please enter a valid email address", "ok");
            return;
        }

        if(!IsValidPassword(password.Text))
        {
            await DisplayAlert("Invalid password", "Must be between 6 and 12 characters", "ok");
            return;
        }

        UserToCreate User = new(name.Text, email.Text, Encoding.ASCII.GetBytes(password.Text));
        await Shell.Current.GoToAsync(nameof(Home));
    }

    bool IsValidEmail(string email)
    {
        var trimmedEmail = email.Trim();

        if (trimmedEmail.EndsWith("."))
        {
            return false;
        }
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == trimmedEmail;
        }
        catch
        {
            return false;
        }
    }



    bool IsValidPassword(string password)
    {
        if (password == null)
            return false;

        if (password.Length < 6 || password.Length > 12)
            return false;

        bool hasUpperCase = password.Any(char.IsUpper);
        bool hasLowerCase = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSymbol = password.Any(ch => !char.IsLetterOrDigit(ch));

        return hasUpperCase && hasLowerCase && hasDigit && hasSymbol;
    }
}