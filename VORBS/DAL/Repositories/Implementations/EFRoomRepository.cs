using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VORBS.Models;

namespace VORBS.DAL.Repositories
{
    public class EFRoomRepository : IRoomRepository
    {
        private VORBSContext db;
        private NLog.Logger _logger;

        public EFRoomRepository(VORBSContext context)
        {
            db = context;
            _logger = NLog.LogManager.GetCurrentClassLogger();
        }

        public Room GetByLocationAndName(Location location, string name)
        {
            return db.Rooms.Where(r => r.LocationID == location.ID && r.RoomName == name).FirstOrDefault();
        }

        public List<Room> GetByLocationName(string locationName)
        {
            return db.Rooms.Where(r => r.Location.Name == locationName).ToList();
        }

        public List<Room> GetByLocationAndSeatCount(Location location, int seatCount)
        {
            return db.Rooms.Where(x => x.Location.ID == location.ID && x.SeatCount >= seatCount && x.Active == true).ToList();
        }

        public List<Room> GetByLocationAndSmartRoom(Location location, bool isSmart)
        {
            return db.Rooms.Where(x => x.Location.ID == location.ID && x.SmartRoom == true && x.Active == true).ToList();
        }

        public List<Room> GetAllRooms()
        {
            return db.Rooms.ToList();
        }

        public Room GetRoomByName(string name)
        {
            return db.Rooms.Where(x => x.RoomName == name).FirstOrDefault();
        }

        public List<Room> GetByStatus(bool status)
        {
            return db.Rooms.Where(x => x.Active == status).ToList();
        }

        public List<Room> GetByLocationAndStatus(Location location, bool status)
        {
            return db.Rooms.Where(r => r.Location.Name == location.Name && r.Active == status).ToList();
        }

        public Room GetRoomById(int id)
        {
            return db.Rooms.Where(x => x.ID == id).FirstOrDefault();
        }

        public void SaveNewRoom(Room room)
        {
            db.Rooms.Add(room);
            db.SaveChanges();
        }

        public void EditRoom(Room room)
        {
            Room originalRoom = GetRoomById(room.ID);

            db.Entry(originalRoom).CurrentValues.SetValues(room);
            db.SaveChanges();
        }
    }
}