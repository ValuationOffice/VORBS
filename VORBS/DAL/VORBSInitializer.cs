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
                new Location(){Name="Location2"},
                new Location(){Name="Location3"},
            };

            locations.ForEach(l => context.Locations.Add(l));
            context.SaveChanges();

            var rooms = new List<Room>()
            {
                new Room(){ LocationID =  1, RoomName = "room-name", ComputerCount = 1, PhoneCount = 1, SeatCount = 8, SmartRoom = true }
                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 0, PhoneCount = 1, SeatCount = 16, SmartRoom = true }
                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 1, PhoneCount = 1, SeatCount = 16, SmartRoom = true }
                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 7, PhoneCount = 1, SeatCount = 16, SmartRoom = true }
                
                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 1, PhoneCount = 1, SeatCount = 4, SmartRoom = true }
                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 1, PhoneCount = 1, SeatCount = 4, SmartRoom = true }
                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 0, PhoneCount = 0, SeatCount = 10, SmartRoom = true }

                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 1, PhoneCount = 1, SeatCount = 8, SmartRoom = true }
                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 1, PhoneCount = 1, SeatCount = 4, SmartRoom = true }
                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 1, PhoneCount = 1, SeatCount = 10, SmartRoom = true }

                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 1, PhoneCount = 1, SeatCount = 6, SmartRoom = true }
                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 0, PhoneCount = 0, SeatCount = 10, SmartRoom = true }
                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 0, PhoneCount = 1, SeatCount = 9, SmartRoom = true }
                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 1, PhoneCount = 1, SeatCount = 8, SmartRoom = true }

                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 1, PhoneCount = 1, SeatCount = 8, SmartRoom = true }
                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 1, PhoneCount = 1, SeatCount = 20, SmartRoom = true }

                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 1, PhoneCount = 1, SeatCount = 12, SmartRoom = true }
                ,new Room(){ LocationID = 1, RoomName = "room-name", ComputerCount = 0, PhoneCount = 1, SeatCount = 12, SmartRoom = true }
            };

            rooms.ForEach(r => context.Rooms.Add(r));
            context.SaveChanges();

            var bookings = new List<Booking>()
            {
                new Booking(){ RoomID = 1, Owner = "User9", StartDate = new DateTime(2015, 5, 13, 9, 0, 0), EndDate = new DateTime(2015, 5, 13, 17, 0, 0) }
                ,new Booking(){ RoomID = 2, Owner = "User9", StartDate = new DateTime(2015, 5, 13, 9, 0, 0), EndDate = new DateTime(2015, 5, 13, 17, 0, 0) }
                ,new Booking(){ RoomID = 3, Owner = "User9", StartDate = new DateTime(2015, 5, 13, 9, 0, 0), EndDate = new DateTime(2015, 5, 13, 17, 0, 0) }
                ,new Booking(){ RoomID = 4, Owner = "User9", StartDate = new DateTime(2015, 5, 13, 9, 0, 0), EndDate = new DateTime(2015, 5, 13, 17, 0, 0) }
                ,new Booking(){ RoomID = 6, Owner = "User9", StartDate = new DateTime(2015, 5, 13, 9, 0, 0), EndDate = new DateTime(2015, 5, 13, 17, 0, 0) }
                ,new Booking(){ RoomID = 7, Owner = "User10", StartDate = new DateTime(2015, 5, 13, 10, 0, 0), EndDate = new DateTime(2015, 5, 13, 14, 0, 0) }
                ,new Booking(){ RoomID = 8, Owner = "User11", StartDate = new DateTime(2015, 5, 13, 10, 30, 0), EndDate = new DateTime(2015, 5, 13, 14, 0, 0) }
                ,new Booking(){ RoomID = 9, Owner = "User12", StartDate = new DateTime(2015, 5, 13, 11, 0, 0), EndDate = new DateTime(2015, 5, 13, 12, 30, 0) }
                ,new Booking(){ RoomID = 10, Owner = "User13", StartDate = new DateTime(2015, 5, 13, 11, 0, 0), EndDate = new DateTime(2015, 5, 13, 12, 30, 0) }
                ,new Booking(){ RoomID = 11, Owner = "User14", StartDate = new DateTime(2015, 5, 13, 9, 0, 0), EndDate = new DateTime(2015, 5, 13, 17, 0, 0) }
                ,new Booking(){ RoomID = 12, Owner = "User15", StartDate = new DateTime(2015, 5, 13, 9, 0, 0), EndDate = new DateTime(2015, 5, 13, 16, 30, 0) }
                ,new Booking(){ RoomID = 13, Owner = "User16", StartDate = new DateTime(2015, 5, 13, 9, 0, 0), EndDate = new DateTime(2015, 5, 13, 17, 0, 0) }
                ,new Booking(){ RoomID = 14, Owner = "User17", StartDate = new DateTime(2015, 5, 13, 9, 0, 0), EndDate = new DateTime(2015, 5, 13, 17, 0, 0) }
                ,new Booking(){ RoomID = 15, Owner = "User18", StartDate = new DateTime(2015, 5, 13, 9, 0, 0), EndDate = new DateTime(2015, 5, 13, 17, 0, 0) }
                ,new Booking(){ RoomID = 16, Owner = "User19", StartDate = new DateTime(2015, 5, 13, 9, 0, 0), EndDate = new DateTime(2015, 5, 13, 17, 0, 0) }
                ,new Booking(){ RoomID = 17, Owner = "User19", StartDate = new DateTime(2015, 5, 13, 9, 0, 0), EndDate = new DateTime(2015, 5, 13, 17, 0, 0) }
                ,new Booking(){ RoomID = 10, Owner = "User20", StartDate = new DateTime(2015, 5, 13, 15, 0, 0), EndDate = new DateTime(2015, 5, 13, 15, 30, 0) }
                ,new Booking(){ RoomID = 18, Owner = "User21", StartDate = new DateTime(2015, 5, 13, 11, 00, 0), EndDate = new DateTime(2015, 5, 13, 12, 30, 0) }
                ,new Booking(){ RoomID = 18, Owner = "User22", StartDate = new DateTime(2015, 5, 13, 13, 00, 0), EndDate = new DateTime(2015, 5, 13, 16, 30, 0) }
                ,new Booking(){ RoomID = 19, Owner = "User23", StartDate = new DateTime(2015, 5, 13, 10, 00, 0), EndDate = new DateTime(2015, 5, 13, 12, 30, 0) }
                ,new Booking(){ RoomID = 19, Owner = "User24", StartDate = new DateTime(2015, 5, 13, 13, 00, 0), EndDate = new DateTime(2015, 5, 13, 14, 30, 0) }
                ,new Booking(){ RoomID = 19, Owner = "User8", StartDate = new DateTime(2015, 5, 13, 15, 00, 0), EndDate = new DateTime(2015, 5, 13, 15, 30, 0) }
                ,new Booking(){ RoomID = 20, Owner = "User25", StartDate = new DateTime(2015, 5, 13, 09, 00, 0), EndDate = new DateTime(2015, 5, 13, 17, 00, 0) }

                ,new Booking(){ RoomID = 2, Owner = "User9", StartDate = new DateTime(2015, 5, 28, 9, 0, 0), EndDate = new DateTime(2015, 5, 28, 17, 0, 0) }
                ,new Booking(){ RoomID = 3, Owner = "User9", StartDate = new DateTime(2015, 5, 28, 9, 0, 0), EndDate = new DateTime(2015, 5, 28, 17, 0, 0) }
                ,new Booking(){ RoomID = 4, Owner = "User9", StartDate = new DateTime(2015, 5, 28, 9, 0, 0), EndDate = new DateTime(2015, 5, 28, 17, 0, 0) }
                ,new Booking(){ RoomID = 7, Owner = "User17", StartDate = new DateTime(2015, 5, 28, 9, 0, 0), EndDate = new DateTime(2015, 5, 28, 17, 0, 0) }
                ,new Booking(){ RoomID = 9, Owner = "User22", StartDate = new DateTime(2015, 5, 28, 11, 30, 0), EndDate = new DateTime(2015, 5, 28, 13, 30, 0) }
                ,new Booking(){ RoomID = 9, Owner = "User13", StartDate = new DateTime(2015, 5, 28, 14, 0, 0), EndDate = new DateTime(2015, 5, 28, 16, 30, 0) }
                ,new Booking(){ RoomID = 10, Owner = "User26", StartDate = new DateTime(2015, 5, 28, 10, 30, 0), EndDate = new DateTime(2015, 5, 28, 11, 00, 0) }
                ,new Booking(){ RoomID = 12, Owner = "User27", StartDate = new DateTime(2015, 5, 28, 13, 30, 0), EndDate = new DateTime(2015, 5, 28, 17, 0, 0) }
                ,new Booking(){ RoomID = 14, Owner = "User17", StartDate = new DateTime(2015, 5, 28, 9, 0, 0), EndDate = new DateTime(2015, 5, 28, 17, 0, 0) }
                ,new Booking(){ RoomID = 15, Owner = "User28", StartDate = new DateTime(2015, 5, 28, 13, 0, 0), EndDate = new DateTime(2015, 5, 28, 14, 30, 0) }
                ,new Booking(){ RoomID = 16, Owner = "User19", StartDate = new DateTime(2015, 5, 28, 9, 0, 0), EndDate = new DateTime(2015, 5, 28, 17, 0, 0) }
                ,new Booking(){ RoomID = 17, Owner = "User19", StartDate = new DateTime(2015, 5, 28, 9, 0, 0), EndDate = new DateTime(2015, 5, 28, 17, 0, 0) }
                ,new Booking(){ RoomID = 18, Owner = "User29", StartDate = new DateTime(2015, 5, 28, 10, 30, 0), EndDate = new DateTime(2015, 5, 28, 11, 30, 0) }
                ,new Booking(){ RoomID = 18, Owner = "User30", StartDate = new DateTime(2015, 5, 28, 12, 0, 0), EndDate = new DateTime(2015, 5, 28, 17, 0, 0) }
                ,new Booking(){ RoomID = 19, Owner = "User31", StartDate = new DateTime(2015, 5, 28, 10, 30, 0), EndDate = new DateTime(2015, 5, 28, 12, 30, 0) }
                ,new Booking(){ RoomID = 20, Owner = "User25", StartDate = new DateTime(2015, 5, 28, 09, 00, 0), EndDate = new DateTime(2015, 5, 28, 17, 00, 0) }
            };

            bookings.ForEach(b => context.Bookings.Add(b));
            context.SaveChanges();

        }
    }
}