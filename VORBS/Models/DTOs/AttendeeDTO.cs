using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VORBS.Models.DTOs
{
    public class AttendeeDTO
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public virtual int BookingId { get; set; }
    }
}