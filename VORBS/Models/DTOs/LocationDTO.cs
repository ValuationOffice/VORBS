using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VORBS.Models.DTOs
{
    public class LocationDTO : Location
    {
        public int ID { get; set; }

        public string Name { get; set; }
    }
}