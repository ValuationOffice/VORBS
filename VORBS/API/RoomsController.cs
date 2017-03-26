using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VORBS.DAL;
using VORBS.DAL.Repositories;
using VORBS.Models;
using VORBS.Models.DTOs;
using VORBS.Services;
using VORBS.Utils;

namespace VORBS.API
{
    [RoutePrefix("api/room")]
    public class RoomsController : ApiController
    {
        private NLog.Logger _logger;
        private VORBSContext db;
        private IDirectoryService _directoryService;
        private IRoomRepository _roomRepository;
        private ILocationRepository _locationRepository;
        private IBookingRepository _bookingRepository;

        public RoomsController(VORBSContext context)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
            db = context;
            _directoryService = new StubbedDirectoryService();
            _locationRepository = new EFLocationRepository(db);
            _roomRepository = new EFRoomRepository(db);
            _bookingRepository = new EFBookingRepository(db);
        }

        public RoomsController() : this(new VORBSContext()) { }
        

        [HttpGet]
        [Route("")]
        public List<RoomDTO> GetAllRooms()
        {
            try
            {
                List<RoomDTO> roomDTOs = new List<RoomDTO>();

                List<Room> rooms = _roomRepository.GetAllRooms().OrderBy(r => r.Location.Name).ThenBy(r => r.RoomName).ToList();

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
        [Route("{roomId:int}")]
        public RoomDTO GetRoomById(int roomId)
        {
            try
            {
                RoomDTO roomDTO = new RoomDTO();

                Room room = _roomRepository.GetRoomById(roomId);

                roomDTO = new RoomDTO()
                {
                    ID = room.ID,
                    location = new LocationDTO()
                    {
                        ID = room.Location.ID,
                        Name = room.Location.Name
                    },
                    RoomName = room.RoomName,
                    ComputerCount = room.ComputerCount,
                    SeatCount = room.SeatCount,
                    PhoneCount = room.PhoneCount,
                    SmartRoom = room.SmartRoom,
                    Active = room.Active
                };

                return roomDTO;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get room using roomid: " + roomId, ex);
                return new RoomDTO();
            }
        }

        [HttpGet]
        [Route("{locationId:int}/{roomName}")]
        public RoomDTO GetRoomByLocationAndName(int locationId, string roomName)
        {
            try
            {
                RoomDTO roomDTO = new RoomDTO();

                Location location = _locationRepository.GetLocationById(locationId);
                Room room = _roomRepository.GetByLocationAndName(location, roomName);

                roomDTO = new RoomDTO()
                {
                    ID = room.ID,
                    location = new LocationDTO()
                    {
                        ID = room.Location.ID,
                        Name = room.Location.Name,
                        LocationCredentials = room.Location.LocationCredentials.ToList().Select(l => { return new LocationCredentialsDTO() { Department = l.Department, Email = l.Email, ID = l.ID, LocationID = l.LocationID, PhoneNumber = l.PhoneNumber }; }).ToList()
                    },
                    RoomName = room.RoomName,
                    ComputerCount = room.ComputerCount,
                    SeatCount = room.SeatCount,
                    PhoneCount = room.PhoneCount,
                    SmartRoom = room.SmartRoom,
                    Active = room.Active
                };

                return roomDTO;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get room using room name: " + roomName, ex);
                return new RoomDTO();
            }
        }

        [HttpGet]
        [Route("{locationName}/{status:int}")]
        public List<RoomDTO> GetRoomsByLocationAndStatus(string locationName, int status)
        {
            try
            {
                Location location = _locationRepository.GetLocationByName(locationName);

                List<RoomDTO> roomDTOs = new List<RoomDTO>();
                List<Room> rooms = new List<Room>();

                if (status < 0)
                    if (locationName == "location")
                        rooms = _roomRepository.GetAllRooms().OrderBy(r => r.Location.Name).ThenBy(r => r.RoomName).ToList();
                    else
                        rooms = _roomRepository.GetByLocationName(locationName).OrderBy(r => r.Location.Name).ThenBy(r => r.RoomName).ToList();
                else
                {
                    bool active = (status == 0) ? false : true;

                    if (locationName == "location")
                        rooms = _roomRepository.GetByStatus(active).OrderBy(r => r.Location.Name).ThenBy(r => r.RoomName).ToList();
                    else
                        rooms =  _roomRepository.GetByLocationAndStatus(location, active).OrderBy(r => r.Location.Name).ThenBy(r => r.RoomName).ToList();
                }

                rooms.ForEach(x => roomDTOs.Add(new RoomDTO()
                {
                    ID = x.ID,
                    location = new LocationDTO()
                    {
                        ID = x.Location.ID,
                        Name = x.Location.Name,
                        LocationCredentials = x.Location.LocationCredentials.ToList().Select(l => { return new LocationCredentialsDTO() { Department = l.Department, Email = l.Email, ID = l.ID, LocationID = l.LocationID, PhoneNumber = l.PhoneNumber }; }).ToList()
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
        [VORBS.Security.VorbsApiAuthoriseAttribute(2)]
        public HttpResponseMessage SaveNewRoom(Room newRoom)
        {
            try
            {
                Location location = _locationRepository.GetLocationById(newRoom.LocationID);

                //Check to see if Room already exists at location
                if (_roomRepository.GetByLocationAndName(location, newRoom.RoomName) != null)
                    return Request.CreateErrorResponse(HttpStatusCode.Conflict, string.Format("Room {0} already exists at {1}.", newRoom.RoomName, location.Name));

                _roomRepository.SaveNewRoom(newRoom);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to add new room.", ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [Route("{existingRoomId:int}")]
        [VORBS.Security.VorbsApiAuthoriseAttribute(2)]
        public HttpResponseMessage EditRoom(int existingRoomId, Room editRoom)
        {
            try
            {
                //Find Existing Booking
                Room existingRoom = _roomRepository.GetRoomById(existingRoomId);
                Location location = _locationRepository.GetLocationById(existingRoom.LocationID);

                if (existingRoom.RoomName != editRoom.RoomName)
                {
                    bool isDuplicate = _roomRepository.GetByLocationAndName(location, editRoom.RoomName) != null;

                    if (isDuplicate)
                        return Request.CreateErrorResponse(HttpStatusCode.Conflict, string.Format("Room {0} already exists at {1}.", editRoom.RoomName, editRoom.Location.Name));
                }

                editRoom.LocationID = existingRoom.LocationID;

                _roomRepository.EditRoom(editRoom);

                _logger.Info("Room successfully Edited: " + editRoom.ID);

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to edit room: " + editRoom.ID, ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpPost]
        [Route("{roomId:Int}/{active:bool}")]
        [VORBS.Security.VorbsApiAuthoriseAttribute(2)]
        public HttpResponseMessage EnableDisableRoom(int roomId, bool active)
        {
            try
            {
                Room room = _roomRepository.GetRoomById(roomId);

                room.Active = active;

                _roomRepository.EditRoom(room);

                if (!active)
                {
                    List<Booking> bookings = _bookingRepository.GetByDateAndRoom(DateTime.Now, room)
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

                    _bookingRepository.Delete(bookings);

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
                string toEmail = _directoryService.GetUser(new User.Pid(ownerBookings[0].PID)).EmailAddress;
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
