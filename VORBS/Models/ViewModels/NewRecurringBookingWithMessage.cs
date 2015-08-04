using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VORBS.Models.ViewModels
{
    public class NewRecurringBookingWithMessage
    {
        public NewRecurringBookingWithMessage(Booking booking, string message)
        {
            Booking = booking;
            RecurringSentence = message;
        }

        public Booking Booking { get; set; }
        public string RecurringSentence { get; set; }
    }
}