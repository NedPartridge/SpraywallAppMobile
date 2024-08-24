namespace SpraywallAppMobile.Models;

// Class holding user data, in preparation to create a new account
class UserToSignUp
{
    // Personal details
    public string Email { get; set; }
    public string Name { get; set; }

    // Hashed password: may be public, because it's already encrypted
    public string Password { get; set; }
}