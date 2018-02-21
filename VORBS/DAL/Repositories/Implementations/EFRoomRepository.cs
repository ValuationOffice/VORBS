using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VORBS.Models;
using VORBS.Utils;

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

            _logger.Trace(LoggerHelper.InitializeClassMessage());
        }

        public Room GetByLocationAndName(Location location, string name)
        {
            Room room = db.Rooms.Where(r => r.LocationID == location.ID && r.RoomName == name).FirstOrDefault();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(room, location, name));

            return room;
        }

        public List<Room> GetByLocationName(string locationName)
        {
            List<Room> rooms = db.Rooms.Where(r => r.Location.Name == locationName).ToList();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(rooms, locationName));

            return rooms;
        }

        public List<Room> GetByLocationAndSeatCount(Location location, int seatCount)
        {
            List<Room> rooms = db.Rooms.Where(x => x.Location.ID == location.ID && x.SeatCount >= seatCount && x.Active == true).ToList();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(rooms, location, seatCount));

            return rooms;
        }

        public List<Room> GetByLocationAndSmartRoom(Location location, bool isSmart)
        {
            List<Room> rooms = db.Rooms.Where(x => x.Location.ID == location.ID && x.SmartRoom == true && x.Active == true).ToList();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(rooms, location, isSmart));

            return rooms;
        }

        public List<Room> GetAllRooms()
        {
            List<Room> rooms = db.Rooms.ToList();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(rooms));

            return rooms;
        }

        public Room GetRoomByName(string name)
        {
            Room room = db.Rooms.Where(x => x.RoomName == name).FirstOrDefault();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(room, name));

            return room;
        }

        public List<Room> GetByStatus(bool status)
        {
            List<Room> rooms = db.Rooms.Where(x => x.Active == status).ToList();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(rooms, status));

            return rooms;
        }

        public List<Room> GetByLocationAndStatus(Location location, bool status)
        {
            List<Room> rooms = db.Rooms.Where(r => r.Location.Name == location.Name && r.Active == status).ToList();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(rooms, location, status));

            return rooms;
        }

        public Room GetRoomById(int id)
        {
            Room room = db.Rooms.Where(x => x.ID == id).FirstOrDefault();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(room, id));

            return room;
        }

        public void SaveNewRoom(Room room)
        {
            try
            {
                db.Rooms.Add(room);
                db.SaveChanges();
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, room));
            }
            catch (Exception exn)
            {
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(exn, room));
                throw exn;
            }

        }

        public void EditRoom(Room room)
        {
            try
            {
                Room originalRoom = GetRoomById(room.ID);

                db.Entry(originalRoom).CurrentValues.SetValues(room);
                db.SaveChanges();

                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, room));
            }
            catch (Exception exn)
            {
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(exn, room));
                throw exn;
            }

        }
    }
}