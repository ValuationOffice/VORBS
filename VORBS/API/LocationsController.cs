using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VORBS.Models;

namespace VORBS.API
{
    [RoutePrefix("api/locations")]
    public class LocationsController : ApiController
    {
        [HttpGet]
        [Route("")]
        public List<Location> GetLocations()
        {
            List<Location> locations = new List<Location>()
            {
                new Location(){ Name = "CEO", Rooms = new string[3]{"MR1", "MR2", "MR3"} },
                new Location(){ Name = "Worthing", Rooms = new string[3]{"MR4", "MR5", "MR6"}  },
                new Location(){ Name = "Kent", Rooms = new string[3]{"MR7", "MR8", "MR9"}  },
                new Location(){ Name = "Leicester", Rooms = new string[3]{"MR10", "MR11", "MR12"}  },
                new Location(){ Name = "Thurrock", Rooms = new string[3]{"MR13", "MR14", "MR15"}  }
            };
            return locations;
        }
    }
}
