using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VORBS.Models.DTOs
{
    public class LocationDTO
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public IEnumerable<RoomDTO> Rooms { get; set; }
    }
}