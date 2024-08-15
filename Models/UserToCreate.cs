namespace SpraywallAppMobile.Models;
class UserToCreate : BaseUser
{
    string Name { get; set; }
    public UserToCreate(string name, string email, byte[] password) : base(email, password)
    {
        Name = name;
    }
}