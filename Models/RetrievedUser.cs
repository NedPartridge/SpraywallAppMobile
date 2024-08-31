namespace SpraywallAppMobile.Models;

// Class representing a 'user' object, retrieved from the server
// 
// Used when initialising user settings, not to be sent back
class RetrievedUser
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}