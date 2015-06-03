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
using System.Data.Entity;

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

                string currentPid = User.Identity.Name.Substring(User.Identity.Name.IndexOf("\\") + 1); //TODO: Change?

                List<Booking> bookings = db.Bookings
                    .Where(x => x.PID == currentPid && x.StartDate >= start).ToList()
                    .OrderBy(x => x.StartDate)
                    .ToList();

                List<BookingDTO> bookingsDTO = new List<BookingDTO>();
                bookings.ForEach(x => bookingsDTO.Add(new BookingDTO()
                {
                    ID = x.ID,
                    EndDate = x.EndDate,
                    StartDate = x.StartDate,
                    Subject = x.Subject,
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

        [Route("{bookingId:int}")]
        [HttpGet]
        public BookingDTO GetRoomBookingsForBookingId(int bookingId)
        {
            try
            {
                Booking booking = db.Bookings.Single(b => b.ID == bookingId);

                BookingDTO bookingsDTO = new BookingDTO()
                {
                    ID = booking.ID,
                    EndDate = booking.EndDate,
                    StartDate = booking.StartDate,
                    Subject = booking.Subject,
                    Owner = booking.Owner,
                    NumberOfAttendees = booking.NumberOfAttendees,
                    ExternalNames = booking.ExternalNames,
                    Flipchart = booking.Flipchart,
                    Projector = booking.Projector,
                    PID = booking.PID,
                    Location = new LocationDTO() { ID = booking.Room.Location.ID, Name = booking.Room.Location.Name },
                    Room = new RoomDTO() { ID = booking.Room.ID, RoomName = booking.Room.RoomName, ComputerCount = booking.Room.ComputerCount, PhoneCount = booking.Room.PhoneCount, SmartRoom = booking.Room.SmartRoom }
                };

                return bookingsDTO;
            }
            catch (Exception ex)
            {
                //TODO: Log Error
                return null;
            }

        }

        [HttpPost]
        public HttpResponseMessage SaveNewBooking(Booking newBooking)
        {
            try
            {
                newBooking.RoomID = db.Rooms.Single(r => r.RoomName == newBooking.Room.RoomName).ID;

                var user = AdQueries.GetUserByCurrentUser(User.Identity.Name);

                newBooking.Owner = user.Name;
                newBooking.PID = user.SamAccountName;

                //Reset Room as we dont want to create another room
                newBooking.Room = null;

                db.Bookings.Add(newBooking);
                db.SaveChanges();

                if (newBooking.Flipchart || newBooking.Projector)
                {
                    //Send Email to DSO
                }

                if (newBooking.ExternalNames != null)
                {
                    //Send Email to secuirty
                }

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                //TODO: Log Exception
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [Route("{existingBookingId:int}")]
        public HttpResponseMessage EditExistingBooking(int existingBookingId, Booking editBooking)
        {
            try
            {
                //Find Existing Booking
                Booking existingBooking = db.Bookings.Single(b => b.ID == existingBookingId);
                editBooking.ID = existingBookingId;

                //Check Room Avaliablity 
                //Only check avaliablity if date/time/attendes has been updated. TODO: Refactor
                if (!(editBooking.StartDate.Equals(existingBooking.StartDate) && editBooking.EndDate.Equals(existingBooking.EndDate) &&
                    editBooking.NumberOfAttendees.Equals(existingBooking.NumberOfAttendees)))
                {
                    AvailabilityController av = new AvailabilityController();
                    Room existingRoom = db.Rooms.First(r => r.ID == existingBooking.RoomID);

                    int roomId = av.GetBestAvaiableRoomForLocation(existingRoom.Location.Name, editBooking.StartDate, editBooking.EndDate, editBooking.NumberOfAttendees, false); //TODO: change smart room

                    if (roomId == 0)
                    {
                        //No Rooms Avaliable
                        return Request.CreateErrorResponse(HttpStatusCode.Conflict, "No Rooms Avaialbe Using Current Date/Time/Attendees.");
                    }

                    if (existingRoom.ID != roomId)
                    {
                        //Change the room id
                        editBooking.RoomID = roomId;
                    }
                }
                else
                    editBooking.RoomID = existingBooking.RoomID;

                //TODO: Maybe change when booking on behalf of user
                editBooking.Owner = existingBooking.Owner;
                editBooking.PID = existingBooking.PID;

                //TODO: Refactor
                if ((existingBooking.Flipchart != editBooking.Flipchart) || (editBooking.Projector != existingBooking.Projector))
                {
                    //Create DSO Email but do not send until db.savechanges
                    //dsoEmailMessage = CreateDSOEmail(editBooking, editBooking.Flipchart, editBooking.Projector);
                }

                db.Entry(existingBooking).CurrentValues.SetValues(editBooking);
                db.SaveChanges();

                //Send DSO Email
                //SendDSOEmail(dsoEmailMessage);

                //Send Owner Email
                //SendOwnerEmail(editBooking);

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

                //TODO: Refactor
                if ((booking.Flipchart || booking.Projector))
                {
                    //Send DSO Email
                    //SendDSOEmail(editBooking, editBooking.Flipchart, editBooking.Projector);
                }

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
