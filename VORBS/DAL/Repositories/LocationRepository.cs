using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VORBS.DAL;
using VORBS.Models;

namespace VORBS.DAL.Repositories
{
    public class LocationRepository
    {
        private VORBSContext db;
        private NLog.Logger _logger;
        public LocationRepository(VORBSContext context)
        {
            db = context;
            _logger = NLog.LogManager.GetCurrentClassLogger();
        }

        public List<Location> GetLocationsByStatus(bool status)
        {
            return db.Locations.Where(x => x.Active == status).ToList();
        }

        public List<Location> GetLocationsWithSmartRooms()
        {
            return db.Locations.Where(x => x.Active == true && x.Rooms.Where(r => r.SmartRoom == true && r.LocationID == x.ID).Count() > 0).ToList();
        }

        public List<Location> GetAllLocations()
        {
            return db.Locations.ToList();
        }

        public Location GetLocationByName(string name)
        {
            return db.Locations.Where(x => x.Name == name).First();
        }

        public Location GetLocationById(int id)
        {
            return db.Locations.Where(x => x.ID == id).First();
        }

        public void SaveNewLocation(Location location)
        {
            db.Locations.Add(location);
            db.SaveChanges();
        }

        public void UpdateLocation(Location location)
        {
            Location originalLocation = GetLocationById(location.ID);

            db.Entry(originalLocation).CurrentValues.SetValues(location);

            //TODO: Need to tidy this up, we shouldn't have to delete all locations and re-add them each time. Not sure if there is a virtual update method
            IEnumerable<LocationCredentials> credentials = db.LocationCredentials.Where(x => x.LocationID == location.ID).ToList();
            db.LocationCredentials.RemoveRange(credentials);
            location.LocationCredentials.ToList().ForEach(x => x.LocationID = location.ID);
            db.LocationCredentials.AddRange(location.LocationCredentials);

            db.SaveChanges();
        }
    }
}