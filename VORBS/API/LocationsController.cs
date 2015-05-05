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
            List<LocationDTO> locations = db.Locations.ToList()
                .Select(l => new LocationDTO() { ID = l.ID, Name = l.Name })
                .ToList();
            return locations;
        }
    }
}
