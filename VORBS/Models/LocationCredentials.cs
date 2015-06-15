using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VORBS.Models
{
    public class LocationCredentials
    {
        public int ID { get; set; }
        public int LocationID { get; set; }
        public string Department { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public virtual Location Location { get; set; }

        public enum DepartmentNames { dss, facilities, security }
    }
}