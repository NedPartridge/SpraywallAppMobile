using SpraywallAppMobile.Models;
using System.Text;
using System.Text.Json;

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
        await Shell.Current.GoToAsync("..");
    }

    // Create a new user account, based on the details provided
    // But first, authenticate the deetz.
    private async void OnSubmitButtonClicked(object sender, TappedEventArgs e)
    {
        if (email.Text == null || password.Text == null || name.Text == null) 
        {
            await DisplayAlert("Invalid entry", "Please fill out all fields", "ok");
            return; 
        }

        if (!IsValidEmail(email.Text))
        {
            await DisplayAlert("Invalid email", "Please enter a valid email address", "ok");
            return;
        }

        if(!IsValidPassword(password.Text))
        {
            await DisplayAlert("Invalid password", "Must be between 6 and 12 characters\nMust use number, letters (upper & lower case), symbols", "ok");
            return;
        }

        UserToCreate User = new(name.Text, email.Text, Encoding.ASCII.GetBytes(password.Text));

        string serialisedUser = JsonSerializer.Serialize(User);
        Console.WriteLine(serialisedUser);


        await Shell.Current.GoToAsync(nameof(Home));
    }


    // Validate email address
    // TODO: Confirm email exists (external service?)
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


    // Confirm password is valid - ie, not 'weak', or beyond storage capabilities. 
    bool IsValidPassword(string password)
    {
        if (password == null)
            return false;

        if (password.Length < 6 || password.Length > 12)
            return false;

        // Using linq instead of regex, for conveniance. 
        bool hasUpperCase = password.Any(char.IsUpper);
        bool hasLowerCase = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSymbol = password.Any(ch => !char.IsLetterOrDigit(ch));

        return hasUpperCase && hasLowerCase && hasDigit && hasSymbol;
    }
}