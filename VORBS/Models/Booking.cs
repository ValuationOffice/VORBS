using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VORBS.Models
{
    public class Booking
    {
        public string Owner { get; set; }
        public string Room { get; set; }
        public Location Location { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}