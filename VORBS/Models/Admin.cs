using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VORBS.Models
{
    public class Admin
    {
        public int Id { get; set; }
        public string pId { get; set; }
        public string Username { get; set; }
        public int PermissionLevel { get; set; }
    }
}