﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpraywallAppMobile.Models;

class ClimbDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string SetterName { get; set; }
    public int Grade { get; set; } // assigned by setter
    // json data representing the holds in a climb
    public string JsonHolds { get; set; }

    // The requesting user's attempts on the climbs
    // Null if not logged
    public int? Attempts { get; set; }
}