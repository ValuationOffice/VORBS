using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace VORBS.Models
{
    public class Booking
    {
        public int ID { get; set; }
        public int RoomID { get; set; }

        public string PID { get; set; }
        public string Owner { get; set; }
        public string Subject { get; set; }

        public virtual Room Room { get; set; }

        public int NumberOfAttendees { get; set; }
        public string ExternalNames { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool Flipchart { get; set; }
        public bool Projector { get; set; }
    }
}

