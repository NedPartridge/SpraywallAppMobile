using SpraywallAppMobile.Helpers;
using SpraywallAppMobile.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SpraywallAppMobile.Pages;

public partial class Settings : ContentPage
{
    HttpClient client;
	public Settings()
	{
		InitializeComponent();
        // Ignore SSL
        var httpClientHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        client = new HttpClient(httpClientHandler);
    }

    // Update the user object with the new data
    private async void OnSaveChanges(object sender, EventArgs e)
    {


        string base64EncryptedString = null;
        // Encrupt the password, construct a user object
        if (!string.IsNullOrEmpty(Password.Text))
        {
            if (!IsValidPassword(Password.Text))
            {
                await DisplayAlert("Invalid password", "Must be between 6 and 20 characters\nMust use number, letters (upper & lower case), symbols", "ok");
                return;
            }
            byte[] passwordArray = Encoding.UTF8.GetBytes(Password.Text);
            byte[] encryptedPassword = SecurityHelper.Encrypt(passwordArray);
            base64EncryptedString = Convert.ToBase64String(encryptedPassword);
        }
        UserToSignUp user = new() { Email = Email.Text, Name = Name.Text, Password = base64EncryptedString };
        
        await UpdateUser(user);
    }
    
    // Send the user home without changing the settings
    private async void OnDiscardChanges(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//" + nameof(Home));
    }

    // Populate the user's settings with data from the server
    // Do not include the current password, for security
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await GetUser();
    }

    // Retrieve information about the current user
    // Set control fields
    private async Task GetUser()
    {
        // retrieve the user as json, manage errors
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppSettings.Token);
        HttpResponseMessage response = await client.GetAsync(AppSettings.absGetUserAddress);
        if(!response.IsSuccessStatusCode) // errors
        {
            await DisplayAlert("Error", "Unknown", "go home");
            await Shell.Current.GoToAsync("//" + nameof(Home));
        }

        // Success!
        string jsonResponse = await response.Content.ReadAsStringAsync();

        // Deserialize the JSON response to an object using System.Text.Json
        var user = JsonSerializer.Deserialize<RetrievedUser>(jsonResponse, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Set control fields
        // Not password, for security - user must update this blindly
        Email.Text = user.Email;
        Name.Text = user.Name;
        Password.Text = null;
    }

    // Update the user on the server
    // Any fields left null in the request will be ignored
    private async Task UpdateUser(UserToSignUp user)
    {
        // Serialize the user, attempt to update the user
        string jsonUser = JsonSerializer.Serialize(user);
        StringContent content = new(jsonUser, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync(AppSettings.absEditUserAddress, content);

        // Manage errors
        if(!response.IsSuccessStatusCode)
        {
            string errorMessage = await response.Content.ReadAsStringAsync();
            await DisplayAlert("Invalid sign up attempt", errorMessage, "ok");
            return;
        }
        // Success! Go to home.
        await Shell.Current.GoToAsync("//" + nameof(Home));
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