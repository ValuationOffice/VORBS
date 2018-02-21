using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VORBS.DAL;
using VORBS.Models;
using VORBS.Utils;

namespace VORBS.DAL.Repositories
{
    public class EFLocationRepository : ILocationRepository
    {
        private VORBSContext db;
        private NLog.Logger _logger;
        public EFLocationRepository(VORBSContext context)
        {
            db = context;
            _logger = NLog.LogManager.GetCurrentClassLogger();
        }

        public List<Location> GetLocationsByStatus(bool status)
        {
            List<Location> locations = db.Locations.Where(x => x.Active == status).ToList();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(locations, status));

            return locations;
        }

        public List<Location> GetLocationsWithSmartRooms()
        {
            List<Location> locations = db.Locations.Where(x => x.Active == true && x.Rooms.Where(r => r.SmartRoom == true && r.LocationID == x.ID).Count() > 0).ToList();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(locations));

            return locations;
        }

        public List<Location> GetAllLocations()
        {
            List<Location> locations = db.Locations.ToList();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(locations));

            return locations;
        }

        public Location GetLocationByName(string name)
        {
            Location location = db.Locations.Where(x => x.Name == name).FirstOrDefault();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(location, name));

            return location;
        }

        public Location GetLocationById(int id)
        {
            Location location = db.Locations.Where(x => x.ID == id).FirstOrDefault();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(location, id));

            return location;
        }

        public void SaveNewLocation(Location location)
        {
            try
            {
                db.Locations.Add(location);
                db.SaveChanges();
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(typeof(void), location));
            }
            catch (Exception exn)
            {
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(exn, location));
                throw exn;
            }
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
            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, location));
        }
    }
}