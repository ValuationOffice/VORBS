using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VORBS.Models;
using VORBS.Models.DTOs;

using VORBS.DAL;
using VORBS.Utils;
using System.Configuration;

namespace VORBS.API
{
    [RoutePrefix("api/locations")]
    public class LocationsController : ApiController
    {
        private NLog.Logger _logger;
        private VORBSContext db = new VORBSContext();

        public LocationsController()
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
        }

        [HttpGet]
        [Route("")]
        public List<LocationDTO> GetLocations()
        {
            List<LocationDTO> locationsDTO = new List<LocationDTO>();

            try
            {
                List<Location> locations = db.Locations.ToList()
                    .ToList();

                locations.ForEach(x => locationsDTO.Add(new LocationDTO()
                {
                    ID = x.ID,
                    Name = x.Name,
                    Active = x.Active,
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
                Location location = db.Locations.Where(x => x.ID == id).SingleOrDefault();
                LocationDTO locationDto = new LocationDTO()
                {
                    Active = location.Active,
                    ID = location.ID,
                    Name = location.Name,
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
                List<Location> locations = db.Locations.Where(x => x.Active == true && x.Rooms.Where(r => r.SmartRoom == true && r.LocationID == x.ID).Count() > 0).ToList();

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
                List<Location> locations = db.Locations.Where(x => x.Active == status).ToList()
                                            .ToList();

                if (extraInfo)
                {
                    locations.ForEach(x => locationsDTO.Add(new LocationDTO()
                    {
                        ID = x.ID,
                        Name = x.Name,
                        Active = x.Active,
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
        public HttpResponseMessage EditLocation(int id, Location editLocation)
        {
            try
            {
                Location originalLoc = db.Locations.Where(x => x.ID == id).SingleOrDefault();
                if (editLocation.Name != originalLoc.Name)
                {
                    int duplicateLocationCheck = db.Locations.Where(l => l.Name == editLocation.Name).Count();
                    if (duplicateLocationCheck > 0)
                        return Request.CreateErrorResponse(HttpStatusCode.Conflict, string.Format("Location {0} already exists.", editLocation.Name));
                }

                db.Entry(originalLoc).CurrentValues.SetValues(editLocation);

                //TODO: Need to tidy this up, we shouldn't have to delete all locations and re-add them each time. Not sure if there is a virtual update method
                IEnumerable<LocationCredentials> credentials = db.LocationCredentials.Where(x => x.LocationID == id).ToList();
                db.LocationCredentials.RemoveRange(credentials);
                editLocation.LocationCredentials.ToList().ForEach(x => x.LocationID = id);
                db.LocationCredentials.AddRange(editLocation.LocationCredentials);

                db.SaveChanges();
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
        public HttpResponseMessage SaveNewLocation(Location newLocation)
        {
            try
            {
                var existingLocation = db.Locations.Any(l => l.ID == newLocation.ID);
                if (!existingLocation)
                {
                    db.Locations.Add(newLocation);
                    db.SaveChanges();

                    _logger.Info("New location sucessfully added: " + newLocation.Name + "/" + newLocation.ID);

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
        public HttpResponseMessage EnableDisableLocation(int locationId, bool active)
        {
            try
            {
                Location location = db.Locations.Single(l => l.ID == locationId);
                location.Active = active;
                db.SaveChanges();

                if (!active)
                {
                    List<Booking> bookings = db.Bookings.Where(b => b.Room.LocationID == locationId && b.StartDate >= DateTime.Now)
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
