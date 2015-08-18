using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VORBS.Models
{
    public class ExternalAttendees
    {
        public int ID { get; set; }
        public int BookingID { get; set; }

        public string FullName { get; set; }
        public string CompanyName { get; set; }
        public bool PassRequired { get; set; }
        
        public virtual Booking Booking { get; set; }
    }
}