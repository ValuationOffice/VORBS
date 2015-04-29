using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VORBS.Models;

using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace VORBS.DAL
{
    public class VORBSContext : DbContext
    {
        public VORBSContext() : base("VORBSContext") { }

        public DbSet<Location> Locations { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }
    }
}