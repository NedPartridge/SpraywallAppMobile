namespace SpraywallAppMobile.Models;

// Class holding user data, in preparation to create a new account
// Null fields to allow use in settings
class UserToSignUp
{
    // Personal details
    public string? Email { get; set; }
    public string? Name { get; set; }

    // Hashed password: may be public, because it's already encrypted
    public string? Password { get; set; }
}