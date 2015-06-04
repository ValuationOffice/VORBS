using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VORBS.Models
{
    public class Admin
    {
        public int ID { get; set; }
        public string PID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Location { get; set; }
        public string Email { get; set; }
        public int PermissionLevel { get; set; }
    }
}