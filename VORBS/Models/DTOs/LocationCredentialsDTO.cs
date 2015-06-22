using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VORBS.Models.DTOs
{
    public class LocationCredentialsDTO
    {
        public int ID { get; set; }
        public int LocationID { get; set; }

        public string Department { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public LocationDTO Location { get; set; }

        public enum DepartmentNames { dss, facilities, security }
    }
}