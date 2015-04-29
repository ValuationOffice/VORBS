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
                new Location(){Name="Location1"},
                new Location(){Name="Location3"},
                new Location(){Name="Kent"},
                new Location(){Name="Leicester"},
                new Location(){Name="Thurrock"}
            };

            locations.ForEach(l => context.Locations.Add(l));
            context.SaveChanges();

            var rooms = new List<Room>()
            {
                new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 1, PhoneCount = 0, SmartRoom = false }
                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 2, PhoneCount = 2, SmartRoom = false }
                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 3, PhoneCount = 1, SmartRoom = false }
                
                ,new Room(){ LocationID = 2, RoomName = "room-name", ComputerCount = 1, PhoneCount = 0, SmartRoom = false }
                ,new Room(){ LocationID = 2, RoomName = "room-name", ComputerCount = 5, PhoneCount = 2, SmartRoom = false }
                ,new Room(){ LocationID = 2, RoomName = "room-name", ComputerCount = 1, PhoneCount = 1, SmartRoom = false }

                ,new Room(){ LocationID = 3, RoomName = "room-name", ComputerCount = 10, PhoneCount = 3, SmartRoom = false }
                ,new Room(){ LocationID = 3, RoomName = "room-name", ComputerCount = 1, PhoneCount = 0, SmartRoom = false }
                ,new Room(){ LocationID = 3, RoomName = "room-name", ComputerCount = 5, PhoneCount = 2, SmartRoom = false }

                ,new Room(){ LocationID = 4, RoomName = "room-name", ComputerCount = 6, PhoneCount = 3, SmartRoom = false }
                ,new Room(){ LocationID = 4, RoomName = "room-name", ComputerCount = 1, PhoneCount = 0, SmartRoom = false }
                ,new Room(){ LocationID = 4, RoomName = "room-name", ComputerCount = 9, PhoneCount = 2, SmartRoom = false }

                ,new Room(){ LocationID = 5, RoomName = "room-name", ComputerCount = 1, PhoneCount = 0, SmartRoom = false }
                ,new Room(){ LocationID = 5, RoomName = "room-name", ComputerCount = 4, PhoneCount = 1, SmartRoom = false }
                ,new Room(){ LocationID = 5, RoomName = "room-name", ComputerCount = 1, PhoneCount = 0, SmartRoom = false }
            };

            rooms.ForEach(r => context.Rooms.Add(r));
            context.SaveChanges();

            var bookings = new List<Booking>()
            {
                new Booking(){ RoomID = 1, Owner = "Admin1", StartDate = DateTime.Now, EndDate = DateTime.Now.AddHours(1) }
                ,new Booking(){ RoomID = 2, Owner = "Admin2", StartDate = DateTime.Now.AddHours(1), EndDate = DateTime.Now.AddHours(2) }
                ,new Booking(){ RoomID = 4, Owner = "Satnam", StartDate = DateTime.Now.AddHours(2), EndDate = DateTime.Now.AddHours(3) }
                ,new Booking(){ RoomID = 5, Owner = "Dave", StartDate = DateTime.Now, EndDate = DateTime.Now.AddHours(1) }
                ,new Booking(){ RoomID = 7, Owner = "Bob", StartDate = DateTime.Now.AddHours(1), EndDate = DateTime.Now.AddHours(2) }
                ,new Booking(){ RoomID = 8, Owner = "Ben", StartDate = DateTime.Now.AddHours(2), EndDate = DateTime.Now.AddHours(3) }
                ,new Booking(){ RoomID = 10, Owner = "Admin5", StartDate = DateTime.Now, EndDate = DateTime.Now.AddHours(1) }
                ,new Booking(){ RoomID = 11, Owner = "Mthoko", StartDate = DateTime.Now.AddHours(1), EndDate = DateTime.Now.AddHours(2) }
                ,new Booking(){ RoomID = 13, Owner = "Admin3", StartDate = DateTime.Now.AddHours(2), EndDate = DateTime.Now.AddHours(3) }
                ,new Booking(){ RoomID = 14, Owner = "Dave", StartDate = DateTime.Now, EndDate = DateTime.Now.AddHours(1) }
            };

            bookings.ForEach(b => context.Bookings.Add(b));
            context.SaveChanges();

        }
    }
}