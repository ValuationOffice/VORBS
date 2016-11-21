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
        public virtual ICollection<ExternalAttendees> ExternalAttendees { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool Flipchart { get; set; }
        public bool Projector { get; set; }

        public bool DssAssist { get; set; }
        public bool IsSmartMeeting { get; set; }
        public int? RecurrenceId { get; set; }

        [NotMapped]
        public DTOs.RecurrenceDTO Recurrence { get; set; }

        [NotMapped]
        public string[] SmartLoactions { get; set; }
    }
}

