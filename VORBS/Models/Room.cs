using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VORBS.Models
{
    public class Room
    {
        public int ID { get; set; }
        public int LocationID { get; set; }

        public string RoomName { get; set; }
        public int ComputerCount { get; set; }
        public int PhoneCount { get; set; }
        public int SeatCount { get; set; }
        public bool SmartRoom { get; set; }

        public virtual Location Location { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
    }
}