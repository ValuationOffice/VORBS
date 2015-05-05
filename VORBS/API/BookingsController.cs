using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using VORBS.Models;
using VORBS.DAL;
using VORBS.Models.DTOs;

namespace VORBS.API
{
    [RoutePrefix("api/bookings")]
    public class BookingsController : ApiController
    {
        private VORBSContext db = new VORBSContext();

        [Route("{start:DateTime}/{end:DateTime}/")]
        [HttpPost]
        public List<BookingDTO> GetRoomBookingsForLocation(Location location, DateTime start, DateTime end)
        {
            //TODO: APPARENTLY, WE CANT MAKE A DTO INSIDE THE PROJECTION .... FIX THIS
            List<BookingDTO> bookings = db.Bookings
                .Where(x => x.StartDate >= start && x.EndDate <= end && x.Room.Location == location)
                .Select(
                    b => new BookingDTO()
                    {
                        ID = b.ID,
                        EndDate = b.EndDate,
                        Owner = b.Owner,
                        StartDate = b.StartDate,
                        Location = new LocationDTO() { Name = b.Room.Location.Name, ID = b.Room.Location.ID },
                        Room = new RoomDTO() { ID = b.Room.ID, RoomName = b.Room.RoomName, ComputerCount = b.Room.ComputerCount, PhoneCount = b.Room.PhoneCount, SmartRoom = b.Room.SmartRoom }
                    })
                .ToList();
            return bookings;
        }

        [Route("{start:DateTime}/{end:DateTime}/{room:int}")]
        [HttpPost]
        public List<BookingDTO> GetRoomBookingsForRoom(Location location, DateTime start, DateTime end, string room)
        {
            List<BookingDTO> bookings = db.Bookings
                .Where(x => x.StartDate >= start && x.EndDate <= end && x.Room.Location == location && x.Room.RoomName == room)
                .Select(
                    b => new BookingDTO()
                    {
                        ID = b.ID,
                        EndDate = b.EndDate,
                        Owner = b.Owner,
                        StartDate = b.StartDate,
                        Location = new LocationDTO() { Name = b.Room.Location.Name, ID = b.Room.Location.ID },
                        Room = new RoomDTO() { ID = b.Room.ID, RoomName = b.Room.RoomName, ComputerCount = b.Room.ComputerCount, PhoneCount = b.Room.PhoneCount, SmartRoom = b.Room.SmartRoom }
                    })
                .ToList();
            return bookings;
        }

        [Route("{start:DateTime}/{end:DateTime}/{room}/{person}")]
        [HttpPost]
        public List<BookingDTO> GetRoomBookingsForRoomAndPerson(Location location, DateTime start, DateTime end, string room, string person)
        {
            List<BookingDTO> bookings = db.Bookings
                .Where(x => x.Owner == person && x.StartDate >= start && x.EndDate <= end && x.Room.Location == location && x.Room.RoomName == room)
                .Select(
                    b => new BookingDTO()
                    {
                        ID = b.ID,
                        EndDate = b.EndDate,
                        Owner = b.Owner,
                        StartDate = b.StartDate,
                        Location = new LocationDTO() { Name = b.Room.Location.Name, ID = b.Room.Location.ID },
                        Room = new RoomDTO() { ID = b.Room.ID, RoomName = b.Room.RoomName, ComputerCount = b.Room.ComputerCount, PhoneCount = b.Room.PhoneCount, SmartRoom = b.Room.SmartRoom }
                    })
                .ToList();
            return bookings;
        }

        [Route("{start:DateTime}/{end:DateTime}/{person}")]
        [HttpGet]
        public List<BookingDTO> GetRoomBookingsForPerson(DateTime start, DateTime end, string person)
        {
            List<BookingDTO> bookings = db.Bookings.Include("Room")
                .Where(x => x.Owner == person && x.StartDate >= start && x.EndDate <= end).ToList()
                .Select(
                    b => new BookingDTO()
                    {
                        ID = b.ID,
                        EndDate = b.EndDate,
                        Owner = b.Owner,
                        StartDate = b.StartDate,
                        Location = new LocationDTO() { Name = b.Room.Location.Name, ID = b.Room.Location.ID },
                        Room = new RoomDTO() { ID = b.Room.ID, RoomName = b.Room.RoomName, ComputerCount = b.Room.ComputerCount, PhoneCount = b.Room.PhoneCount, SmartRoom = b.Room.SmartRoom }
                    })
                .ToList();
            return bookings;
        }
    }
}
