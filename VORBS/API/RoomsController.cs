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
using VORBS.Utils.interfaces;
using static VORBS.Services.RoomService;

namespace VORBS.API
{
    [RoutePrefix("api/room")]
    public class RoomsController : ApiController
    {
        private NLog.Logger _logger;

        private IDirectoryService _directoryService;
        private RoomService _roomService;

        private IRoomRepository _roomRepository;
        private ILocationRepository _locationRepository;
        private IBookingRepository _bookingRepository;

        public RoomsController(IBookingRepository bookingRepository, ILocationRepository locationRepository, IRoomRepository roomRepository, IDirectoryService directoryService, EmailHelper emailHelper)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();

            _directoryService = directoryService;
            _roomService = new RoomService(_logger, locationRepository, roomRepository, bookingRepository, directoryService, emailHelper);

            _locationRepository = locationRepository;
            _roomRepository = roomRepository;
            _bookingRepository = bookingRepository;

            _logger.Trace(LoggerHelper.InitializeClassMessage());
        }

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
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(roomDTOs));
                return roomDTOs;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get rooms.", ex);
                var result = new List<RoomDTO>();
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(result));
                return result;
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
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(roomDTO, roomId));
                return roomDTO;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get room using roomid: " + roomId, ex);
                var result = new RoomDTO();
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(result, roomId));
                return result;
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
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(roomDTO, locationId, roomName));
                return roomDTO;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get room using room name: " + roomName, ex);
                var result = new RoomDTO();
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(result, locationId, roomName));
                return result;
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

                if (locationName == "location")
                    _logger.Debug("LocationName is \"location\"");

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
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(roomDTOs, locationName, status));
                return roomDTOs;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get rooms.", ex);
                var results = new List<RoomDTO>();
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(results, locationName, status));
                return results;
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

        [HttpPut]
        [Route("{existingRoomId:int}")]
        [VORBS.Security.VorbsApiAuthoriseAttribute(2)]
        public HttpResponseMessage EditRoom(int existingRoomId, Room editRoom)
        {
            try
            {
                Room existingRoom = _roomRepository.GetRoomById(existingRoomId);

                _roomService.EditRoom(existingRoom, editRoom);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (RoomExistsException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Conflict, string.Format("Room {0} already exists at {1}.", editRoom.RoomName, editRoom.Location.Name));
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to edit room: " + editRoom.ID, ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpPatch]
        [Route("{roomId:Int}/{active:bool}")]
        [VORBS.Security.VorbsApiAuthoriseAttribute(2)]
        public HttpResponseMessage EnableDisableRoom(int roomId, bool active)
        {
            try
            {
                Room room = _roomRepository.GetRoomById(roomId);

                _roomService.ToggleRoomActive(room, active, ConfigurationManager.AppSettings);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to enable/disable room.", ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }


    }
}
