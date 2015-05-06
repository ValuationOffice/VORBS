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
        VORBSContext db = new VORBSContext();

        [HttpGet]
        [Route("")]
        public List<LocationDTO> GetLocations()
        {
            List<LocationDTO> locationsDTO = new List<LocationDTO>();
            List<Location> locations = db.Locations.ToList()
                .ToList();

            locations.ForEach(x => locationsDTO.Add(new LocationDTO()
            {
                ID = x.ID,
                Name = x.Name,
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

            return locationsDTO;
        }
    }
}
