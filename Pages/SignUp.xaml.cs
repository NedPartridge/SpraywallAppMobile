using SpraywallAppMobile.Helpers;
using SpraywallAppMobile.Models;
using System.Text;
using System.Text.Json;

namespace SpraywallAppMobile.Pages;

public partial class SignUp : ContentPage
{
    HttpClient httpClient = new HttpClient();
    // Initialise the component - only applicable to certain deployment platforms.
    public SignUp()
	{
		InitializeComponent();
	}

    // Exit the sign up screen, go back to the signup/login choice
    private async void OnBackButtonClicked(object sender, TappedEventArgs e)
    {
        // Cannot route directly to page: instead, 'step' 'back' one page
        await Shell.Current.GoToAsync("..");
    }


    // Create a new user account, based on the details provided
    // Authenticate locally first, submit request, handle responses
    private async void OnSubmitButtonClicked(object sender, TappedEventArgs e)
    {
        // Validate data locally
        if (email.Text == null || password.Text == null || name.Text == null) 
        {
            await DisplayAlert("Invalid entry", "Please fill out all fields", "ok");
            return; 
        }

        if(!IsValidPassword(password.Text))
        {
            await DisplayAlert("Invalid password", "Must be between 6 and 20 characters\nMust use number, letters (upper & lower case), symbols", "ok");
            return;
        }

        // Encrypt the password, construct a user object
        byte[] passwordArray = Encoding.UTF8.GetBytes(password.Text);
        byte[] encryptedPassword = SecurityHelper.Encrypt(passwordArray);
        string base64EncryptedString = Convert.ToBase64String(encryptedPassword);
        UserToSignUp user = new() { Email = email.Text, Name = name.Text, Password = base64EncryptedString };
        
        // Serialise the user, attempt to create an account
        StringContent jsonUser = new(JsonSerializer.Serialize(user));
        HttpResponseMessage response = await httpClient.PostAsync(AppSettings.SignUpAddress, jsonUser);

        // If succesfull, set the token and go to the home page
        if(response.IsSuccessStatusCode)
        {
            AppSettings.Token = response.Content.ToString();
            await Shell.Current.GoToAsync(nameof(Home));
        }
        // Respond to bad login details
        else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            await DisplayAlert("Invalid sign up attempt", response.Content.ToString(), "ok");
            return;
        }
        // Respond to other errors
        else
        {
            await DisplayAlert("Something's gone wrong", "We don't know what, please try again", "ok");
            return;
        }
    }

    // Confirm password is valid - ie, not 'weak', or beyond storage capabilities
    // Done on frontend to save processing a bad password.
    bool IsValidPassword(string password)
    {
        // Existance check
        if (password == null)
            return false;

        // Range check
        if (password.Length < 6 || password.Length > 20)
            return false;

        // Using linq instead of regex, for conveniance and readability. 
        // Test strength
        bool hasUpperCase = password.Any(char.IsUpper);
        bool hasLowerCase = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSymbol = password.Any(ch => !char.IsLetterOrDigit(ch));

        return hasUpperCase && hasLowerCase && hasDigit && hasSymbol;
    }
}