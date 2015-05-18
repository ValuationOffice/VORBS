using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VORBS.Models
{
    public class Attendee
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public int BookingId { get; set; }
    }
}