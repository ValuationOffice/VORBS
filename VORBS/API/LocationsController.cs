using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VORBS.Models;
using VORBS.Models.DTOs;

using VORBS.DAL;

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
                    Rooms = x.Rooms.Select(r =>
                    {
                        RoomDTO rDto = new RoomDTO()
                            {
                                ID = r.ID,
                                RoomName = r.RoomName,
                                ComputerCount = r.ComputerCount,
                                PhoneCount = r.PhoneCount,
                                SmartRoom = r.SmartRoom
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
        [Route("{status:bool}")]
        public List<LocationDTO> GetLocationsByStatus(bool status)
        {
            List<LocationDTO> locationsDTO = new List<LocationDTO>();

            try
            {
                List<Location> locations = db.Locations.Where(x => x.Active == status).ToList()
                                            .ToList();
                locations.ForEach(x => locationsDTO.Add(new LocationDTO()
                {
                    ID = x.ID,
                    Name = x.Name,
                    Active = x.Active,
                    Rooms = x.Rooms.Select(r =>
                    {
                        RoomDTO rDto = new RoomDTO()
                            {
                                ID = r.ID,
                                RoomName = r.RoomName,
                                ComputerCount = r.ComputerCount,
                                PhoneCount = r.PhoneCount,
                                SmartRoom = r.SmartRoom
                            }; return rDto;
                    }).ToList()
                }));
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get list of locations", ex);
            }
            return locationsDTO;
        }

        [HttpPost]
        [Route("")]
        public HttpResponseMessage SaveNewLocation(Location newLocation)
        {
            try
            {
                db.Locations.Add(newLocation);
                db.SaveChanges();

                _logger.Info("New location sucessfully added: " + newLocation.Name + "/" + newLocation.ID);

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to create new location: " + newLocation.Name, ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
