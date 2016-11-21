using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VORBS.Models.ViewModels
{
    public class FacilitiesEdittedBooking
    {
        public Booking OriginalBooking { get; set; }
        public Booking EdittedBooking { get; set; }
        public IEnumerable<string> EquipmentRemoved { get; set; }
        public IEnumerable<string> EquipmentAdded { get; set; }
    }
}