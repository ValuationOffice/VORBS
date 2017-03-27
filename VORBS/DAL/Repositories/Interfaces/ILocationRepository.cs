using System.Collections.Generic;
using VORBS.Models;

namespace VORBS.DAL.Repositories
{
    public interface ILocationRepository
    {
        List<Location> GetAllLocations();
        Location GetLocationById(int id);
        Location GetLocationByName(string name);
        List<Location> GetLocationsByStatus(bool status);
        List<Location> GetLocationsWithSmartRooms();
        void SaveNewLocation(Location location);
        void UpdateLocation(Location location);
    }
}