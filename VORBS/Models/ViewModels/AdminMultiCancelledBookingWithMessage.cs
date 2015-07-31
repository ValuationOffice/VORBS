using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VORBS.Models.ViewModels
{
    public class AdminMultiCancelledBookingWithMessage
    {
        public AdminMultiCancelledBookingWithMessage(IEnumerable<Booking> bookings, string message)
        {
            Bookings = bookings;
            CancellationMessage = message;
        }
        public IEnumerable<Booking> Bookings { get; set; }
        public string CancellationMessage { get; set; }
    }
}