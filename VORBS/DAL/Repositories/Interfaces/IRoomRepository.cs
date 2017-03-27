using System.Collections.Generic;
using VORBS.Models;

namespace VORBS.DAL.Repositories
{
    public interface IRoomRepository
    {
        void EditRoom(Room room);
        List<Room> GetAllRooms();
        Room GetByLocationAndName(Location location, string name);
        List<Room> GetByLocationAndStatus(Location location, bool status);
        List<Room> GetByLocationName(string locationName);
        List<Room> GetByLocationAndSeatCount(Location location, int seatCount);
        List<Room> GetByLocationAndSmartRoom(Location location, bool isSmart);
        List<Room> GetByStatus(bool status);
        Room GetRoomById(int id);
        Room GetRoomByName(string name);
        void SaveNewRoom(Room room);
    }
}