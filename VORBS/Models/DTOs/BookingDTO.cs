using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VORBS.Models.DTOs
{
    public class BookingDTO
    {
        public int ID { get; set; }

        public string Owner { get; set; }

        public RoomDTO Room { get; set; }
        public LocationDTO Location { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}