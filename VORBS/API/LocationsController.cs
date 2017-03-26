using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VORBS.Models;
using VORBS.Models.DTOs;

using VORBS.DAL;
using VORBS.DAL.Repositories;
using VORBS.Utils;
using System.Configuration;
using VORBS.Services;

namespace VORBS.API
{
    [RoutePrefix("api/locations")]
    public class LocationsController : ApiController
    {
        private NLog.Logger _logger;
        private VORBSContext db;
        private IDirectoryService _directoryService;
        private ILocationRepository _locationRepository;
        private IBookingRepository _bookingRepository;

        public LocationsController(VORBSContext context)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
            db = context;
            _locationRepository = new EFLocationRepository(db);
            _directoryService = new StubbedDirectoryService();
            _bookingRepository = new EFBookingRepository(db);
        }

        public LocationsController() : this(new VORBSContext()) { }
        

        [HttpGet]
        [Route("")]
        public List<LocationDTO> GetLocations()
        {
            List<LocationDTO> locationsDTO = new List<LocationDTO>();

            try
            {
                List<Location> locations = _locationRepository.GetAllLocations();

                locations.ForEach(x => locationsDTO.Add(new LocationDTO()
                {
                    ID = x.ID,
                    Name = x.Name,
                    Active = x.Active,
                    AdditionalInformation = x.AdditionalInformation,
                    LocationCredentials = x.LocationCredentials.Select(lc =>
                    {
                        LocationCredentialsDTO lcDto = new LocationCredentialsDTO()
                        {
                            ID = lc.ID,
                            Department = lc.Department,
                            LocationID = x.ID,
                            Email = lc.Email,
                            PhoneNumber = lc.PhoneNumber,
                        }; return lcDto;
                    }).ToList(),
                    Rooms = x.Rooms.Select(r =>
                    {
                        RoomDTO rDto = new RoomDTO()
                            {
                                ID = r.ID,
                                RoomName = r.RoomName,
                                ComputerCount = r.ComputerCount,
                                PhoneCount = r.PhoneCount,
                                SmartRoom = r.SmartRoom,
                                SeatCount = r.SeatCount,
                                Active = r.Active
                            }; return rDto;
                    }).ToList()
                }));
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get list of locations: ", ex);
            }
            return locationsDTO;
        }

        [HttpGet]
        [Route("{id:int}")]
        public LocationDTO GetLocationById(int id)
        {
            try
            {
                Location location = _locationRepository.GetLocationById(id);
                LocationDTO locationDto = new LocationDTO()
                {
                    Active = location.Active,
                    ID = location.ID,
                    Name = location.Name,
                    AdditionalInformation = location.AdditionalInformation,
                    Rooms = location.Rooms.Select(r =>
                    {
                        RoomDTO rDto = new RoomDTO()
                        {
                            ID = r.ID,
                            RoomName = r.RoomName,
                            ComputerCount = r.ComputerCount,
                            PhoneCount = r.PhoneCount,
                            SmartRoom = r.SmartRoom,
                            SeatCount = r.SeatCount
                        }; return rDto;
                    }).ToList(),
                    LocationCredentials = location.LocationCredentials.Select(l =>
                    {
                        LocationCredentialsDTO lcDto = new LocationCredentialsDTO()
                        {
                            Department = l.Department,
                            Email = l.Email,
                            ID = location.ID,
                            LocationID = l.LocationID,
                            PhoneNumber = l.PhoneNumber
                        }; return lcDto;
                    }).ToList()
                };
                return locationDto;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get a location by id: " + id, ex);
                return new LocationDTO();
            }
        }

        [HttpGet]
        [Route("{searchLoaction}")]
        public List<string> GetSmartRoomLoactions(string searchLoaction)
        {
            List<string> locationNames = new List<string>();

            try
            {
                List<Location> locations = _locationRepository.GetLocationsWithSmartRooms();

                if (locations.Exists(l => l.Name == searchLoaction))
                {
                    if (locations.Single(l => l.Name == searchLoaction).Rooms.Count() <= 1)
                        locations.Remove(locations.Single(l => l.Name == searchLoaction));
                }

                foreach (var loc in locations)
                {
                    foreach (var room in loc.Rooms.Where(r => r.SmartRoom == true && r.Active))
                        locationNames.Add(loc.Name);
                }

            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get list of smart locations", ex);
            }
            return locationNames;
        }

        [HttpGet]
        [Route("{status:bool}/{extraInfo:bool}")]
        public List<LocationDTO> GetLocationsByStatus(bool status, bool extraInfo)
        {
            List<LocationDTO> locationsDTO = new List<LocationDTO>();

            try
            {
                List<Location> locations = _locationRepository.GetLocationsByStatus(status);

                if (extraInfo)
                {
                    locations.ForEach(x => locationsDTO.Add(new LocationDTO()
                    {
                        ID = x.ID,
                        Name = x.Name,
                        Active = x.Active,
                        AdditionalInformation = x.AdditionalInformation,
                        LocationCredentials = x.LocationCredentials.Select(lc =>
                        {
                            LocationCredentialsDTO lcDto = new LocationCredentialsDTO()
                            {
                                ID = lc.ID,
                                Department = lc.Department,
                                LocationID = x.ID,
                                Email = lc.Email,
                                PhoneNumber = lc.PhoneNumber,
                            }; return lcDto;
                        }).ToList(),
                        Rooms = x.Rooms.Select(r =>
                        {
                            RoomDTO rDto = new RoomDTO()
                            {
                                ID = r.ID,
                                RoomName = r.RoomName,
                                ComputerCount = r.ComputerCount,
                                PhoneCount = r.PhoneCount,
                                SmartRoom = r.SmartRoom,
                                SeatCount = r.SeatCount,
                                Active = r.Active
                            }; return rDto;
                        }).ToList()
                    }));
                }
                else
                {
                    locations.ForEach(x => locationsDTO.Add(new LocationDTO()
                    {
                        ID = x.ID,
                        Name = x.Name,
                        Active = x.Active,
                        //Only get smart rooms as we need them for SMART bookings.
                        Rooms = x.Rooms.Where(y => y.SmartRoom == true && y.Active == true).Select(r =>
                        {
                            RoomDTO rDto = new RoomDTO()
                            {
                                ID = r.ID,
                                RoomName = r.RoomName,
                                ComputerCount = r.ComputerCount,
                                PhoneCount = r.PhoneCount,
                                SmartRoom = r.SmartRoom,
                                SeatCount = r.SeatCount,
                                Active = r.Active
                            }; return rDto;
                        }).ToList()
                    }));
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get list of locations", ex);
            }

            return locationsDTO;
        }

        [HttpPut]
        [Route("{id:int}")]
        [VORBS.Security.VorbsApiAuthoriseAttribute(2)]
        public HttpResponseMessage EditLocation(int id, Location editLocation)
        {
            try
            {
                Location originalLoc = _locationRepository.GetLocationById(id);
                if (editLocation.Name != originalLoc.Name)
                {
                    bool exists = _locationRepository.GetLocationByName(editLocation.Name) != null;
                    if (exists)
                        return Request.CreateErrorResponse(HttpStatusCode.Conflict, string.Format("Location {0} already exists.", editLocation.Name));
                }
                editLocation.ID = id;

                _locationRepository.UpdateLocation(editLocation);

                _logger.Info(string.Format("Location {0} successfully editted", id));
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to edit the location: " + editLocation.Name, ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [Route("")]
        [VORBS.Security.VorbsApiAuthoriseAttribute(2)]
        public HttpResponseMessage SaveNewLocation(Location newLocation)
        {
            try
            {
                var existingLocation = _locationRepository.GetLocationById(newLocation.ID);
                if (existingLocation == null)
                {
                    _locationRepository.SaveNewLocation(newLocation);

                    _logger.Info("New location successfully added: " + newLocation.Name + "/" + newLocation.ID);

                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }

            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to create new location: " + newLocation.Name, ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [Route("{locationId:Int}/{active:bool}")]
        [VORBS.Security.VorbsApiAuthoriseAttribute(2)]
        public HttpResponseMessage EnableDisableLocation(int locationId, bool active)
        {
            try
            {
                Location location = _locationRepository.GetLocationById(locationId);
                location.Active = active;

                _locationRepository.UpdateLocation(location);

                if (!active)
                {
                    List<Booking> bookings = _bookingRepository.GetByDateAndLocation(DateTime.Now, location)
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
