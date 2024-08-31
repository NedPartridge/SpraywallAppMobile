using SpraywallAppMobile.Helpers;
using SpraywallAppMobile.Models;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace SpraywallAppMobile.Pages;

public partial class SignUp : ContentPage
{
    HttpClient httpClient;
    // Initialise the component - only applicable to certain deployment platforms.
    public SignUp()
	{
		InitializeComponent();
        // Ignore SSL
        var httpClientHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        httpClient = new HttpClient(httpClientHandler);
    }

    // Exit the sign up screen, go back to the signup/login choice
    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        // Popup should be enabled to begin with
        Overlay.IsVisible = true;

        // Cannot route directly to page: instead, 'step' 'back' one page
        await Shell.Current.GoToAsync("//" + nameof(MainPage));
    }

    // Data usage consent agreement signed, so close the popup
    private async void OnConsent(object sender, EventArgs e)
    {
        Overlay.IsVisible = false;
    }

    // Create a new user account, based on the details provided
    // Authenticate locally first, submit request, handle responses
    private async void OnSubmitButtonClicked(object sender, EventArgs e)
    {
        // Validate data locally
        if (string.IsNullOrEmpty(email.Text) || string.IsNullOrEmpty(password.Text) || string.IsNullOrEmpty(name.Text))
        {
            await DisplayAlert("Invalid entry", "Please fill out all fields", "ok");
            return;
        }

        if (!IsValidPassword(password.Text))
        {
            await DisplayAlert("Invalid password", "Must be between 6 and 20 characters\nMust use number, letters (upper & lower case), symbols", "ok");
            return;
        }

        // Encrypt the password, construct a user object
        byte[] encryptedBytesPassword = SecurityHelper.Encrypt(Encoding.UTF8.GetBytes(password.Text));
        string encryptedBase64Password = Convert.ToBase64String(encryptedBytesPassword);
        UserToSignUp user = new() { Email = email.Text, Name = name.Text, Password = encryptedBase64Password };


        // Serialize the user, attempt to create an account
        string jsonUser = JsonSerializer.Serialize(user);
        StringContent content = new(jsonUser, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await httpClient.PostAsync(AppSettings.absSignUpAddress, content);

        // If successful, set the token and go to the home page
        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            AppSettings.Token = responseBody;
            await Shell.Current.GoToAsync("//" + nameof(Home));
        }
        // Respond to bad login details
        else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            string errorMessage = await response.Content.ReadAsStringAsync();
            await DisplayAlert("Invalid sign up attempt", errorMessage, "ok");
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