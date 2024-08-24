namespace SpraywallAppMobile.Models;

// Class holding user data, in preparation to make a log in attempt
class UserToLogIn
{
    // Personal details
    public string Email { get; set; }

    // Hashed password: may be public, because it's already encrypted
    public string Password { get; set; }
}
