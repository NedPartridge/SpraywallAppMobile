namespace SpraywallAppMobile.Models;

// Represents a wall, retrieved from the server
// Name for ui purposes, id for future requests
class WallDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
}