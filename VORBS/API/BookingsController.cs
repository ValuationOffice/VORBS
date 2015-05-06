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
            if (location == null)
                return new List<BookingDTO>();

            List<Booking> bookings = db.Bookings
                .Where(x => x.StartDate >= start && x.EndDate <= end && x.Room.Location.ID == location.ID)
                .ToList();

            List<BookingDTO> bookingsDTO = new List<BookingDTO>();
            bookings.ForEach(x => bookingsDTO.Add(new BookingDTO()
            {
                ID = x.ID,
                EndDate = x.EndDate,
                StartDate = x.StartDate,
                Owner = x.Owner,
                Location = new LocationDTO() { ID = x.Room.Location.ID, Name = x.Room.Location.Name },
                Room = new RoomDTO() { ID = x.Room.ID, RoomName = x.Room.RoomName, ComputerCount = x.Room.ComputerCount, PhoneCount = x.Room.PhoneCount, SmartRoom = x.Room.SmartRoom }
            }));


            return bookingsDTO;
        }

        [Route("{start:DateTime}/{end:DateTime}/{room}")]
        [HttpPost]
        public List<BookingDTO> GetRoomBookingsForRoom(Location location, DateTime start, DateTime end, string room)
        {
            if (location == null || room == null)
                return new List<BookingDTO>();

            List<Booking> bookings = db.Bookings
                .Where(x => x.StartDate >= start && x.EndDate <= end && x.Room.Location.ID == location.ID && x.Room.RoomName == room)
                .ToList();

            List<BookingDTO> bookingsDTO = new List<BookingDTO>();
            bookings.ForEach(x => bookingsDTO.Add(new BookingDTO()
            {
                ID = x.ID,
                EndDate = x.EndDate,
                StartDate = x.StartDate,
                Owner = x.Owner,
                Location = new LocationDTO() { ID = x.Room.Location.ID, Name = x.Room.Location.Name },
                Room = new RoomDTO() { ID = x.Room.ID, RoomName = x.Room.RoomName, ComputerCount = x.Room.ComputerCount, PhoneCount = x.Room.PhoneCount, SmartRoom = x.Room.SmartRoom }
            }));


            return bookingsDTO;
        }

        [Route("{start:DateTime}/{end:DateTime}/{room}/{person}")]
        [HttpPost]
        public List<BookingDTO> GetRoomBookingsForRoomAndPerson(Location location, DateTime start, DateTime end, string room, string person)
        {
            if (location == null || room == null || person == null)
                return new List<BookingDTO>();

            List<Booking> bookings = db.Bookings
                .Where(x => x.Owner == person && x.StartDate >= start && x.EndDate <= end && x.Room.Location.ID == location.ID && x.Room.RoomName == room)
                .ToList();

            List<BookingDTO> bookingsDTO = new List<BookingDTO>();
            bookings.ForEach(x => bookingsDTO.Add(new BookingDTO()
            {
                ID = x.ID,
                EndDate = x.EndDate,
                StartDate = x.StartDate,
                Owner = x.Owner,
                Location = new LocationDTO() { ID = x.Room.Location.ID, Name = x.Room.Location.Name },
                Room = new RoomDTO() { ID = x.Room.ID, RoomName = x.Room.RoomName, ComputerCount = x.Room.ComputerCount, PhoneCount = x.Room.PhoneCount, SmartRoom = x.Room.SmartRoom }
            }));


            return bookingsDTO;
        }

        [Route("{start:DateTime}/{end:DateTime}/{person}")]
        [HttpGet]
        public List<BookingDTO> GetRoomBookingsForPerson(DateTime start, DateTime end, string person)
        {
            if (person == null)
                return new List<BookingDTO>();

            List<Booking> bookings = db.Bookings
                .Where(x => x.Owner == person && x.StartDate >= start && x.EndDate <= end).ToList()
                .ToList();

            List<BookingDTO> bookingsDTO = new List<BookingDTO>();
            bookings.ForEach(x => bookingsDTO.Add(new BookingDTO()
            {
                ID = x.ID,
                EndDate = x.EndDate,
                StartDate = x.StartDate,
                Owner = x.Owner,
                Location = new LocationDTO() { ID = x.Room.Location.ID, Name = x.Room.Location.Name },
                Room = new RoomDTO() { ID = x.Room.ID, RoomName = x.Room.RoomName, ComputerCount = x.Room.ComputerCount, PhoneCount = x.Room.PhoneCount, SmartRoom = x.Room.SmartRoom }
            }));


            return bookingsDTO;
        }
    }
}
