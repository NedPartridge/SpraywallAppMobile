using SpraywallAppMobile.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpraywallAppMobile.Models;

// Class holding a set of holds, and descriptive parameters
class Climb
{
    // The holds that make up the climb
    public List<SkiaHold> Holds { get; set; } = new();

    // Descriptive properties
    public string? Name { get; set; }
    public string? SetterName { get; set; } // Static string - account name changes will not effect this. 
    public int Grade { get; set; } // Between 0 and 15, to be visually prefixed be 'V' (the scale is named after John 'Vermin' Sherman)
}