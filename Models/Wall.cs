using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpraywallAppMobile.Models;

// Represents a wall
// Retrieved from the server, relationship data is not included, but requested/managed separately.
class Wall
{
    public int Id { get; set; }
    public string Name { get; set; }


    // Cannot process an image file and json in the same request, so the
    // image is encoded as base64, sent, recived, THEN saved at the path
    public string ImagePath { get; set; }

    // path of saved json data
    public string JsonPath { get; set; }
}
