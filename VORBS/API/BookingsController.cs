using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using VORBS.Models;
using VORBS.DAL;
using VORBS.Models.DTOs;

using VORBS.Utils;
using System.Diagnostics;
using System.IO;

namespace VORBS.API
{
    [RoutePrefix("api/bookings")]
    public class BookingsController : ApiController
    {
        private VORBSContext db = new VORBSContext();

        [Route("{location}/{start:DateTime}/{end:DateTime}/")]
        [HttpGet]
        public List<BookingDTO> GetRoomBookingsForLocation(string location, DateTime start, DateTime end)
        {
            if (location == null)
                return new List<BookingDTO>();

            List<Booking> bookings = db.Bookings
                .Where(x => x.StartDate >= start && x.EndDate <= end && x.Room.Location.Name == location)
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

        [Route("{location}/{room}/{start:DateTime}/{end:DateTime}")]
        [HttpGet]
        public List<BookingDTO> GetRoomBookingsForRoom(string location, DateTime start, DateTime end, string room)
        {
            if (location == null || room == null)
                return new List<BookingDTO>();

            List<Booking> bookings = db.Bookings
                .Where(x => x.StartDate >= start && x.EndDate <= end && x.Room.Location.Name == location && x.Room.RoomName == room)
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

        [Route("{location}/{room}/{start:DateTime}/{end:DateTime}/{person}")]
        [HttpGet]
        public List<BookingDTO> GetRoomBookingsForRoomAndPerson(string location, DateTime start, DateTime end, string room, string person)
        {
            if (location == null || room == null || person == null)
                return new List<BookingDTO>();

            List<Booking> bookings = db.Bookings
                .Where(x => x.Owner == person && x.StartDate >= start && x.EndDate <= end && x.Room.Location.Name == location && x.Room.RoomName == room)
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

        [Route("{start:DateTime}/{person}")]
        [HttpGet]
        public List<BookingDTO> GetAllRoomBookingsForCurrentUser(DateTime start, string person)
        {

            try
            {
                if (User.Identity.Name == null)
                    return new List<BookingDTO>();

                var user = AdQueries.GetUserByCurrentUser(User.Identity.Name);

                List<Booking> bookings = db.Bookings
                    .Where(x => x.Owner == user.SamAccountName && x.StartDate >= start).ToList()
                    .OrderBy(x => x.StartDate)
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
            catch (Exception ex)
            {
                //TODO: Log Error
                return null;
            }

        }

        public HttpResponseMessage SaveNewBooking([FromBody] Booking newBooking)
        {
             try
             {
                 newBooking.RoomID = db.Rooms.Single(r => r.RoomName == newBooking.Room.RoomName).ID;
                 newBooking.Owner = AdQueries.GetUserByCurrentUser(User.Identity.Name).SamAccountName;

                 //Reset Room as we dont want to create another room
                 newBooking.Room = null;

                 db.Bookings.Add(newBooking);
                 db.SaveChanges();

                 if (newBooking.PC || newBooking.Flipchart || newBooking.Projector)
                 {
                     //Send Email to DSO
                 }

                 if (newBooking.ExternalNames != null)
                 {
                     //Send Email to secuirty
                 }

                 //Send Meeting Request to all Attnedees


                 return new HttpResponseMessage(HttpStatusCode.OK);
             }
             catch (Exception ex)
             {
                 //TODO: Log Exception
                 return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
             }
        }

        [Route("{bookingId:Int}")]
        [HttpDelete]
        public HttpResponseMessage DeleteBookingById(int bookingId)
        {
            try
            {
                Booking booking = db.Bookings.First(b => b.ID == bookingId);

                db.Bookings.Remove(booking);
                db.SaveChanges();

                //Once Booking has been removed; Send Cancealtion Emails
                //if (booking.Attendes.Count > 0)
                //{
                //    //Outlook.SendCancellationRequest("7220451", "7220393");
                //}

                //if (booking.Equipment.Count > 0)
                //{
                //    //Outlook.SendCancelEquipmentMail("7220451", "7220393");
                //}

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                //TODO: Log Exception
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
