using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpraywallAppMobile.Models;

// For the logbook
// A climb, with it's name, identifier, and a reference to it's parent wall.
class ClimbDto2
{
    public int Id { get; set; }
    public string Name { get; set; }

    public int WallID { get; set; }
    public string WallName { get; set; }
}