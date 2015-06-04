using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VORBS.Models.DTOs
{
    public class AdminDTO
    {
        public int Id { get; set; }
        public string pId { get; set; }
        public string Username { get; set; }
        public int PermissionLevel { get; set; }
    }
}