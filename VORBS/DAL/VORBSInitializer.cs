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
                new Location(){Name="Location1", Active= true },
                new Location(){Name="Location2", Active= true },
                new Location(){Name="Location3", Active= true },
            };

            locations.ForEach(l => context.Locations.Add(l));
            context.SaveChanges();

            var admins = new List<Admin>()
            {
                new Admin(){ ID = 1, FirstName = "Admin1", LastName = "Admin1", Location = "Location1", Email = "fakeemail1@mail.com", PermissionLevel = 2, PID = "0000001" },
                new Admin(){ ID = 2, FirstName = "Admin2", LastName = "Admin2", Location = "Location1", Email = "fakeemail2@mail.com", PermissionLevel = 2, PID = "0000002" },
                new Admin(){ ID = 3, FirstName = "Admin3", LastName = "Admin3", Location = "Location1", Email = "fakeemail3@mail.com", PermissionLevel = 2, PID = "0000003" },
                new Admin(){ ID = 4, FirstName = "Admin4", LastName = "Admin4", Location = "Location1", Email = "fakeemail4@mail.com", PermissionLevel = 2, PID = "0000004" },
                new Admin(){ ID = 5, FirstName = "Admin5", LastName = "Admin5", Location = "Location1", Email = "fakeemail5@mail.com", PermissionLevel = 2, PID = "0000005" },
                new Admin(){ ID = 5, FirstName = "Admin6", LastName = "Admin6", Location = "Location1", Email = "fakeemail6@mail.com", PermissionLevel = 2, PID = "0000006" }
            };

            admins.ForEach(a => context.Admins.Add(a));
            context.SaveChanges();

            var rooms = new List<Room>()
            {
                new Room(){ LocationID =  1, RoomName = "room-name", ComputerCount = 1, PhoneCount = 1, SeatCount = 8, SmartRoom = false,Active = true }
                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 0, PhoneCount = 1, SeatCount = 16, SmartRoom = false, Active = true }
                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 1, PhoneCount = 1, SeatCount = 16, SmartRoom = false, Active = true }
                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 7, PhoneCount = 1, SeatCount = 16, SmartRoom = false, Active = true }
                
                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 1, PhoneCount = 1, SeatCount = 4, SmartRoom = false, Active = true }
                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 1, PhoneCount = 1, SeatCount = 4, SmartRoom = false, Active = true }
                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 0, PhoneCount = 0, SeatCount = 10, SmartRoom = false, Active = true }

                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 1, PhoneCount = 1, SeatCount = 8, SmartRoom = false, Active = true }
                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 1, PhoneCount = 1, SeatCount = 4, SmartRoom = false, Active = true }
                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 1, PhoneCount = 1, SeatCount = 10, SmartRoom = false, Active = true }

                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 1, PhoneCount = 1, SeatCount = 6, SmartRoom = false, Active = true }
                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 0, PhoneCount = 0, SeatCount = 10, SmartRoom = false, Active = true }
                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 0, PhoneCount = 1, SeatCount = 9, SmartRoom = false, Active = true }
                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 1, PhoneCount = 1, SeatCount = 8, SmartRoom = false, Active = true }

                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 1, PhoneCount = 1, SeatCount = 8, SmartRoom = false, Active = true }

                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 1, PhoneCount = 1, SeatCount = 12, SmartRoom = false, Active = true }
                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 0, PhoneCount = 1, SeatCount = 12, SmartRoom = false, Active = true }

                ,new Room(){ LocationID = 2, RoomName = "room-name", ComputerCount = 0, PhoneCount = 0, SeatCount = 6, SmartRoom = false, Active = true }
                ,new Room(){ LocationID = 2, RoomName = "room-name", ComputerCount = 0, PhoneCount = 0, SeatCount = 3, SmartRoom = false, Active = true }
                ,new Room(){ LocationID = 2, RoomName = "room-name", ComputerCount = 0, PhoneCount = 0, SeatCount = 3, SmartRoom = false, Active = true }

                ,new Room(){ LocationID = 2, RoomName = "room-name", ComputerCount = 0, PhoneCount = 0, SeatCount = 6, SmartRoom = false, Active = true }
                ,new Room(){ LocationID = 2, RoomName = "room-name", ComputerCount = 0, PhoneCount = 0, SeatCount = 3, SmartRoom = false, Active = true }
                ,new Room(){ LocationID = 2, RoomName = "room-name", ComputerCount = 0, PhoneCount = 0, SeatCount = 3, SmartRoom = false, Active = true }
                ,new Room(){ LocationID = 2, RoomName = "room-name", ComputerCount = 0, PhoneCount = 0, SeatCount = 24, SmartRoom = true, Active = true  }

                ,new Room(){ LocationID = 2, RoomName = "room-name", ComputerCount = 0, PhoneCount = 0, SeatCount = 24, SmartRoom = true, Active = true  }

                ,new Room(){ LocationID = 3, RoomName = "room-name", ComputerCount = 12, PhoneCount = 0, SeatCount = 12, SmartRoom = false, Active = true }
                ,new Room(){ LocationID = 3, RoomName = "room-name", ComputerCount = 0, PhoneCount = 0, SeatCount = 18, SmartRoom = true, Active = true }
                ,new Room(){ LocationID = 3, RoomName = "room-name", ComputerCount = 0, PhoneCount = 0, SeatCount = 6, SmartRoom = false, Active = true }

                ,new Room(){ LocationID = 3, RoomName = "room-name", ComputerCount = 1, PhoneCount = 0, SeatCount = 8, SmartRoom = false, Active = true }
                ,new Room(){ LocationID = 3, RoomName = "room-name", ComputerCount = 0, PhoneCount = 0, SeatCount = 8, SmartRoom = false, Active = true }
                ,new Room(){ LocationID = 3, RoomName = "room-name", ComputerCount = 1, PhoneCount = 0, SeatCount = 4, SmartRoom = false, Active = true }
            };

            rooms.ForEach(r => context.Rooms.Add(r));
            context.SaveChanges();

            var bookings = new List<Booking>()
            {
                new Booking(){ RoomID = 1, Owner = "Admin3 Admin3", StartDate = new DateTime(2015, 5, 13, 9, 0, 0), EndDate = new DateTime(2015, 5, 13, 17, 0, 0) }
                ,new Booking(){ RoomID = 2, Owner = "Admin3 Admin3", StartDate = new DateTime(2015, 5, 13, 9, 0, 0), EndDate = new DateTime(2015, 5, 13, 17, 0, 0) }
                ,new Booking(){ RoomID = 3, Owner = "Admin3 Admin3", StartDate = new DateTime(2015, 5, 13, 9, 0, 0), EndDate = new DateTime(2015, 5, 13, 17, 0, 0) }
                ,new Booking(){ RoomID = 4, Owner = "Admin3 Admin3", StartDate = new DateTime(2015, 5, 13, 9, 0, 0), EndDate = new DateTime(2015, 5, 13, 17, 0, 0) }
                ,new Booking(){ RoomID = 6, Owner = "Admin3 Admin3", StartDate = new DateTime(2015, 5, 13, 9, 0, 0), EndDate = new DateTime(2015, 5, 13, 17, 0, 0) }
                ,new Booking(){ RoomID = 7, Owner = "User2", StartDate = new DateTime(2015, 5, 13, 10, 0, 0), EndDate = new DateTime(2015, 5, 13, 14, 0, 0) }
                ,new Booking(){ RoomID = 8, Owner = "Admin2 Admin2", StartDate = new DateTime(2015, 5, 13, 10, 30, 0), EndDate = new DateTime(2015, 5, 13, 14, 0, 0) }
                ,new Booking(){ RoomID = 9, Owner = "User4", StartDate = new DateTime(2015, 5, 13, 11, 0, 0), EndDate = new DateTime(2015, 5, 13, 12, 30, 0) }
                ,new Booking(){ RoomID = 10, Owner = "User5", StartDate = new DateTime(2015, 5, 13, 11, 0, 0), EndDate = new DateTime(2015, 5, 13, 12, 30, 0) }
                ,new Booking(){ RoomID = 11, Owner = "User6", StartDate = new DateTime(2015, 5, 13, 9, 0, 0), EndDate = new DateTime(2015, 5, 13, 17, 0, 0) }
                ,new Booking(){ RoomID = 12, Owner = "User6", StartDate = new DateTime(2015, 5, 13, 9, 0, 0), EndDate = new DateTime(2015, 5, 13, 16, 30, 0) }
                ,new Booking(){ RoomID = 13, Owner = "User5", StartDate = new DateTime(2015, 5, 13, 9, 0, 0), EndDate = new DateTime(2015, 5, 13, 17, 0, 0) }
                ,new Booking(){ RoomID = 14, Owner = "User7", StartDate = new DateTime(2015, 5, 13, 9, 0, 0), EndDate = new DateTime(2015, 5, 13, 17, 0, 0) }
                ,new Booking(){ RoomID = 15, Owner = "User5", StartDate = new DateTime(2015, 5, 13, 9, 0, 0), EndDate = new DateTime(2015, 5, 13, 17, 0, 0) }
                ,new Booking(){ RoomID = 16, Owner = "User6", StartDate = new DateTime(2015, 5, 13, 9, 0, 0), EndDate = new DateTime(2015, 5, 13, 17, 0, 0) }
                ,new Booking(){ RoomID = 17, Owner = "User6", StartDate = new DateTime(2015, 5, 13, 9, 0, 0), EndDate = new DateTime(2015, 5, 13, 17, 0, 0) }
                ,new Booking(){ RoomID = 10, Owner = "User5", StartDate = new DateTime(2015, 5, 13, 15, 0, 0), EndDate = new DateTime(2015, 5, 13, 15, 30, 0) }
                ,new Booking(){ RoomID = 18, Owner = "Admin3 Admin3", StartDate = new DateTime(2015, 5, 13, 11, 00, 0), EndDate = new DateTime(2015, 5, 13, 12, 30, 0) }
                ,new Booking(){ RoomID = 18, Owner = "User4", StartDate = new DateTime(2015, 5, 13, 13, 00, 0), EndDate = new DateTime(2015, 5, 13, 16, 30, 0) }
                ,new Booking(){ RoomID = 19, Owner = "User4", StartDate = new DateTime(2015, 5, 13, 10, 00, 0), EndDate = new DateTime(2015, 5, 13, 12, 30, 0) }
                ,new Booking(){ RoomID = 19, Owner = "User6", StartDate = new DateTime(2015, 5, 13, 13, 00, 0), EndDate = new DateTime(2015, 5, 13, 14, 30, 0) }
                ,new Booking(){ RoomID = 19, Owner = "User8", StartDate = new DateTime(2015, 5, 13, 15, 00, 0), EndDate = new DateTime(2015, 5, 13, 15, 30, 0) }
                ,new Booking(){ RoomID = 20, Owner = "User4", StartDate = new DateTime(2015, 5, 13, 09, 00, 0), EndDate = new DateTime(2015, 5, 13, 17, 00, 0) }

                ,new Booking(){ RoomID = 2, Owner = "Admin3 Admin3", StartDate = new DateTime(2015, 5, 28, 9, 0, 0), EndDate = new DateTime(2015, 5, 28, 17, 0, 0) }
                ,new Booking(){ RoomID = 3, Owner = "Admin3 Admin3", StartDate = new DateTime(2015, 5, 28, 9, 0, 0), EndDate = new DateTime(2015, 5, 28, 17, 0, 0) }
                ,new Booking(){ RoomID = 4, Owner = "Admin3 Admin3", StartDate = new DateTime(2015, 5, 28, 9, 0, 0), EndDate = new DateTime(2015, 5, 28, 17, 0, 0) }
                ,new Booking(){ RoomID = 7, Owner = "User7", StartDate = new DateTime(2015, 5, 28, 9, 0, 0), EndDate = new DateTime(2015, 5, 28, 17, 0, 0) }
                ,new Booking(){ RoomID = 9, Owner = "User2", StartDate = new DateTime(2015, 5, 28, 11, 30, 0), EndDate = new DateTime(2015, 5, 28, 13, 30, 0) }
                ,new Booking(){ RoomID = 9, Owner = "User4", StartDate = new DateTime(2015, 5, 28, 14, 0, 0), EndDate = new DateTime(2015, 5, 28, 16, 30, 0) }
                ,new Booking(){ RoomID = 10, Owner = "User4", StartDate = new DateTime(2015, 5, 28, 10, 30, 0), EndDate = new DateTime(2015, 5, 28, 11, 00, 0) }
                ,new Booking(){ RoomID = 12, Owner = "Admin2 Admin2", StartDate = new DateTime(2015, 5, 28, 13, 30, 0), EndDate = new DateTime(2015, 5, 28, 17, 0, 0) }
                ,new Booking(){ RoomID = 14, Owner = "User7", StartDate = new DateTime(2015, 5, 28, 9, 0, 0), EndDate = new DateTime(2015, 5, 28, 17, 0, 0) }
                ,new Booking(){ RoomID = 15, Owner = "Admin2 Admin2", StartDate = new DateTime(2015, 5, 28, 13, 0, 0), EndDate = new DateTime(2015, 5, 28, 14, 30, 0) }
                ,new Booking(){ RoomID = 16, Owner = "User6", StartDate = new DateTime(2015, 5, 28, 9, 0, 0), EndDate = new DateTime(2015, 5, 28, 17, 0, 0) }
                ,new Booking(){ RoomID = 17, Owner = "User6", StartDate = new DateTime(2015, 5, 28, 9, 0, 0), EndDate = new DateTime(2015, 5, 28, 17, 0, 0) }
                ,new Booking(){ RoomID = 18, Owner = "User2", StartDate = new DateTime(2015, 5, 28, 10, 30, 0), EndDate = new DateTime(2015, 5, 28, 11, 30, 0) }
                ,new Booking(){ RoomID = 18, Owner = "User5", StartDate = new DateTime(2015, 5, 28, 12, 0, 0), EndDate = new DateTime(2015, 5, 28, 17, 0, 0) }
                ,new Booking(){ RoomID = 19, Owner = "User4", StartDate = new DateTime(2015, 5, 28, 10, 30, 0), EndDate = new DateTime(2015, 5, 28, 12, 30, 0) }
                ,new Booking(){ RoomID = 20, Owner = "User4", StartDate = new DateTime(2015, 5, 28, 09, 00, 0), EndDate = new DateTime(2015, 5, 28, 17, 00, 0) }
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