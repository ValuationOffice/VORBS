using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VORBS.DAL;
using VORBS.Models;
using VORBS.Models.DTOs;
using VORBS.Utils;

namespace VORBS.API
{
    [RoutePrefix("api/room")]
    public class RoomsController : ApiController
    {
        private NLog.Logger _logger;
        private VORBSContext db = new VORBSContext();

        public RoomsController()
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
        }


        [HttpGet]
        [Route("")]
        public List<RoomDTO> GetAllRooms()
        {
            try
            {
                List<RoomDTO> roomDTOs = new List<RoomDTO>();

                List<Room> rooms = db.Rooms.OrderBy(r => r.Location.Name).ThenBy(r => r.RoomName).ToList();

                rooms.ForEach(x => roomDTOs.Add(new RoomDTO()
                {
                    ID = x.ID,
                    location = new LocationDTO()
                    {
                        ID = x.Location.ID,
                        Name = x.Location.Name
                    },
                    RoomName = x.RoomName,
                    ComputerCount = x.ComputerCount,
                    SeatCount = x.SeatCount,
                    PhoneCount = x.PhoneCount,
                    SmartRoom = x.SmartRoom,
                    Active = x.Active
                }));

                return roomDTOs;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get rooms.", ex);
                return new List<RoomDTO>();
            }
        }

        [HttpGet]
        [Route("{locationName}/{status:int}")]
        public List<RoomDTO> GetRoomsByLocationAndStatus(string locationName, int status)
        {
            try
            {
                List<RoomDTO> roomDTOs = new List<RoomDTO>();
                List<Room> rooms = new List<Room>();

                if (status < 0)
                    if (locationName == "location")
                        rooms = db.Rooms.OrderBy(r => r.Location.Name).ThenBy(r => r.RoomName).ToList();
                    else
                        rooms = db.Rooms.Where(r => r.Location.Name == locationName).OrderBy(r => r.Location.Name).ThenBy(r => r.RoomName).ToList();
                else
                {
                    bool active = (status == 0) ? false : true;

                    if (locationName == "location")
                        rooms = db.Rooms.Where(r => r.Active == active).OrderBy(r => r.Location.Name).ThenBy(r => r.RoomName).ToList();
                    else
                        rooms = db.Rooms.Where(r => r.Location.Name == locationName && r.Active == active).OrderBy(r => r.Location.Name).ThenBy(r => r.RoomName).ToList();
                }

                rooms.ForEach(x => roomDTOs.Add(new RoomDTO()
                {
                    ID = x.ID,
                    location = new LocationDTO()
                    {
                        ID = x.Location.ID,
                        Name = x.Location.Name
                    },
                    RoomName = x.RoomName,
                    ComputerCount = x.ComputerCount,
                    SeatCount = x.SeatCount,
                    PhoneCount = x.PhoneCount,
                    SmartRoom = x.SmartRoom,
                    Active = x.Active
                }));

                return roomDTOs;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get rooms.", ex);
                return new List<RoomDTO>();
            }
        }

        [Route("")]
        [HttpPost]
        public HttpResponseMessage SaveNewRoom(Room newRoom)
        {
            try
            {
                //Check to see if Room already exists at location
                if (db.Rooms.Where(r => r.RoomName == newRoom.RoomName && r.LocationID == newRoom.LocationID).Count() > 0)
                    return Request.CreateErrorResponse(HttpStatusCode.Conflict, string.Format("Room {0} already exists at {1}.", newRoom.RoomName, newRoom.Location.Name));

                newRoom.Active = true;

                db.Rooms.Add(newRoom);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to add new room.", ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [Route("{roomId:Int}/{active:bool}")]
        public HttpResponseMessage EnableDisableRoom(int roomId, bool active)
        {
            try
            {
                Room room = db.Rooms.Single(r => r.ID == roomId);

                room.Active = active;
                db.SaveChanges();

                if (!active)
                {
                    List<Booking> bookings = db.Bookings.Where(b => b.RoomID == roomId && b.StartDate >= DateTime.Now)
                                                        .OrderBy(b => b.Owner)
                                                        .ToList();

                    if (bookings.Count() < 1)
                        return Request.CreateResponse(HttpStatusCode.OK);

                    List<Booking> ownerBookings = new List<Booking>();

                    string currentOwner = bookings[0].Owner;

                    foreach (var booking in bookings)
                    {
                        if (booking.Owner != currentOwner)
                        {
                            SendMultiplyBookingEmail(ownerBookings);

                            ownerBookings = new List<Booking>();
                            currentOwner = booking.Owner;
                        }

                        ownerBookings.Add(booking);
                    }

                    //Final Send the last owner bookings
                    SendMultiplyBookingEmail(ownerBookings);

                    db.Bookings.RemoveRange(bookings);
                    db.SaveChanges();
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to enable/disable room.", ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        private void SendMultiplyBookingEmail(List<Booking> ownerBookings)
        {
            try
            {
                //Once Booking has been removed; Send Cancelltion Emails
                string toEmail = AdQueries.GetUserByPid(ownerBookings[0].PID).EmailAddress;
                string body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/AdminMultiCancelledBooking.cshtml", ownerBookings);

                Utils.EmailHelper.SendEmail(ConfigurationManager.AppSettings["fromEmail"], toEmail, "Meeting room booking(s) cancellation", body);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to send admin multiple bookings email.", ex);
            }
        }
    }
}
