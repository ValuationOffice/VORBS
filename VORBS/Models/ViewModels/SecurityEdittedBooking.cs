using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VORBS.Models.ViewModels
{
    public class SecurityEdittedBooking
    {
        public Booking EdittedBooking { get; set; }
        public IEnumerable<ExternalAttendees> NewAttendees { get; set; }
        public IEnumerable<ExternalAttendees> RemovedAttendees { get; set; }
    }
}