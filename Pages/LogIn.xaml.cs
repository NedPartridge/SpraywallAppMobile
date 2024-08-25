using SpraywallAppMobile.Helpers;
using SpraywallAppMobile.Models;
using System.Text;
using System.Text.Json;

namespace SpraywallAppMobile.Pages;


// Code behind class for the login page
public partial class LogIn : ContentPage
{
    HttpClient httpClient = new HttpClient();

    // Initialise the component - only applicable to certain deployment platforms.
    public LogIn()
	{
		InitializeComponent();
    }

    // Exit the login screen, go back to the signup/login choice 
    // Do NOT set the currently logged in user.
    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        // Cannot route directly to page: instead, 'step' 'back' one page
        await Shell.Current.GoToAsync("//" + nameof(MainPage));
    }

    // Login the user, by checking provided details against API.
    private async void OnLoginButtonClicked(object sender, EventArgs e)
    {
        // Validate data locally
        if (string.IsNullOrEmpty(email.Text) || string.IsNullOrEmpty(password.Text))
        {
            await DisplayAlert("Invalid entry", "Please fill out all fields", "ok");
            return;
        }

        // Encrupt the password, construct a user object
        byte[] passwordArray = Encoding.UTF8.GetBytes(password.Text);
        byte[] encryptedPassword = SecurityHelper.Encrypt(passwordArray);
        string base64EncryptedString = Convert.ToBase64String(encryptedPassword);

        UserToLogIn user = new() { Email = email.Text, Password = base64EncryptedString };

        // Serialise the user, attempt to log in
        string jsonUser = JsonSerializer.Serialize(user);
        StringContent content = new(jsonUser, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await httpClient.PostAsync(AppSettings.absLogInAddress, content);

        // If succesfull, set the token and go to the home page
        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            AppSettings.Token = responseBody;
            await Shell.Current.GoToAsync("//" + nameof(Home));
        }
        // Respond to bad login details
        else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            await DisplayAlert("Failed login attempt", await response.Content.ReadAsStringAsync(), "ok");
            return;
        }
        // Respond to other errors
        else
        {
            await DisplayAlert("Something's gone wrong", "We don't know what, please try again", "ok");
            return;
        }
    }
}