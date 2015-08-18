using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VORBS.Models.DTOs
{
    public class BookingDTO
    {
        public int ID { get; set; }

        public string PID { get; set; }
        public string Owner { get; set; }
        public string Subject { get; set; }

        public RoomDTO Room { get; set; }
        public LocationDTO Location { get; set; }

        public int NumberOfAttendees { get; set; }
        public IEnumerable<ExternalAttendeesDTO> ExternalAttendees { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool Flipchart { get; set; }
        public bool Projector { get; set; }

        public bool DssAssist { get; set; }
        public bool IsSmartMeeting { get; set; }
    }
}