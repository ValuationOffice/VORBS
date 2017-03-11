using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.Entity;
using VORBS.Models;

namespace VORBS.DAL
{
    public class VORBSInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<VORBSContext>
    {
        protected override void Seed(VORBSContext context)
        {
            var locations = new List<Location>()
            {
                new Location(){ Name="Location1", Active= true }
            };

            locations.ForEach(l => context.Locations.Add(l));
            context.SaveChanges();

            var admins = new List<Admin>()
            {
                new Admin(){ ID = 1, FirstName = "Admin1", LastName = "Admin1", LocationID = 1, Email = "fakeemail1@mail.com", PermissionLevel = 2, PID = "0000001" }
            };

            admins.ForEach(a => context.Admins.Add(a));
            context.SaveChanges();

            var rooms = new List<Room>()
            {
                new Room(){ LocationID =  1, RoomName = "Room1", ComputerCount = 1, PhoneCount = 1, SeatCount = 8, SmartRoom = false, Active = true }
            };

            rooms.ForEach(r => context.Rooms.Add(r));
            context.SaveChanges();

            var bookings = new List<Booking>()
            {

            };

            int dayOffset = 1;
            foreach (var b in bookings.Take(22))
            {
                if (new int[2] { 6, 0 }.ToList().Contains(((int)b.StartDate.AddDays((DateTime.Now.Date - b.StartDate.Date).Days + 1).DayOfWeek)))
                    dayOffset = dayOffset + 3;

                b.StartDate =  b.StartDate.AddDays((DateTime.Now.Date - b.StartDate.Date).Days + 1);
                b.EndDate = b.EndDate.AddDays((DateTime.Now.Date - b.EndDate.Date).Days + 1);
            }

            foreach (var b in bookings.Skip(22))
            {
                if (new int[2] { 6, 0 }.ToList().Contains(((int)b.StartDate.AddDays((DateTime.Now.Date - b.StartDate.Date).Days + 1).DayOfWeek)))
                    dayOffset = dayOffset + 3;

                b.StartDate = b.StartDate.AddDays((DateTime.Now.Date - b.StartDate.Date).Days + 2);
                b.EndDate = b.EndDate.AddDays((DateTime.Now.Date - b.EndDate.Date).Days + 2);
            }

            bookings.ForEach(b => context.Bookings.Add(b));
            context.SaveChanges();
        }
    }
}