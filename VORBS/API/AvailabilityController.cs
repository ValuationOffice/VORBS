﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VORBS.Models;
using VORBS.DAL;
using VORBS.Models.DTOs;
using VORBS.Utils;

namespace VORBS.API
{
    [RoutePrefix("api/availability")]
    public class AvailabilityController : ApiController
    {
        private NLog.Logger _logger;
        private VORBSContext db = new VORBSContext();

        public AvailabilityController()
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
        }

        [HttpGet]
        [Route("{location}/{start:DateTime}")]
        public List<RoomDTO> GetAllRoomBookingsForLocation(string location, DateTime start)
        {
            List<RoomDTO> rooms = new List<RoomDTO>();

            if (location == null)
                return new List<RoomDTO>();

            List<Room> roomData = new List<Room>();

            try
            {
                var availableRooms = db.Rooms.Where(x => x.Location.Name == location)
                    .OrderBy(r => r.SeatCount)
                    .ToList();

                roomData.AddRange(availableRooms);

                roomData.ForEach(x => rooms.Add(new RoomDTO()
                {
                    ID = x.ID,
                    RoomName = x.RoomName,
                    PhoneCount = x.PhoneCount,
                    ComputerCount = x.ComputerCount,
                    SeatCount = x.SeatCount,
                    SmartRoom = x.SmartRoom,
                    Bookings = x.Bookings.Where(b => b.StartDate.Date == start.Date).Select(b =>
                    {
                        BookingDTO bDto = new BookingDTO()
                        {
                            ID = b.ID,
                            Owner = b.Owner,
                            StartDate = b.StartDate,
                            EndDate = b.EndDate
                        };
                        return bDto;
                    }).ToList()
                }));
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get available rooms for location: " + location, ex);
            }
            return rooms;
        }

        [HttpGet]
        [Route("{location}/{start:DateTime}/{end:DateTime}")]
        public List<RoomDTO> GetAvailableRoomsForLocation(string location, DateTime start, DateTime end)
        {

            List<RoomDTO> rooms = new List<RoomDTO>();

            if (location == null)
                return new List<RoomDTO>();

            List<Room> roomData = new List<Room>();

            try
            {
                var locationRooms = db.Rooms.Where(x => x.Location.Name == location && x.SeatCount >= 5).ToList();
                var availableRooms = db.Rooms.Where(x =>
                    x.Location.Name == location
                    //&& x.SeatCount >= 5
                    //&& (x.Bookings.Where(b => b.StartDate < end && start < b.EndDate)).Count() == 0
                ).ToList();

                roomData.AddRange(availableRooms);

                roomData.ForEach(x => rooms.Add(new RoomDTO()
                {
                    ID = x.ID,
                    RoomName = x.RoomName,
                    PhoneCount = x.PhoneCount,
                    ComputerCount = x.ComputerCount,
                    SmartRoom = x.SmartRoom,
                    SeatCount = x.SeatCount,
                    Bookings = x.Bookings.Where(b => b.StartDate.Date == start.Date && b.EndDate.Date == end.Date).Select(b =>
                    {
                        BookingDTO bDto = new BookingDTO()
                        {
                            ID = b.ID,
                            Owner = b.Owner,
                            StartDate = b.StartDate,
                            EndDate = b.EndDate
                        };
                        return bDto;
                    }).ToList()
                }));
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get available rooms for location: " + location, ex);
            }
            return rooms;
        }

        
        [HttpGet]
        [Route("{location}/{start:DateTime}/{numberOfPeople:int}/{smartRoom:bool}")]
        public List<RoomDTO> GetAvailableRoomsForLocation(string location, DateTime start, int numberOfPeople, bool smartRoom)
        {
            List<RoomDTO> rooms = new List<RoomDTO>();

            if (location == null)
                return new List<RoomDTO>();

            List<Room> roomData = new List<Room>();

            try
            {
                var locationRooms = db.Rooms.Where(x => x.Location.Name == location && x.SeatCount >= numberOfPeople).ToList();
                var availableRooms = db.Rooms.Where(x =>
                    x.Location.Name == location
                    && x.SeatCount >= numberOfPeople
                    //&& (x.Bookings.Where(b => start < b.EndDate)).Count() == 0
                ).ToList();

                roomData.AddRange(availableRooms);

                roomData.ForEach(x => rooms.Add(new RoomDTO()
                {
                    ID = x.ID,
                    RoomName = x.RoomName,
                    PhoneCount = x.PhoneCount,
                    ComputerCount = x.ComputerCount,
                    SmartRoom = x.SmartRoom,
                    Bookings = x.Bookings.Select(b =>
                    {
                        BookingDTO bDto = new BookingDTO()
                        {
                            ID = b.ID,
                            Owner = b.Owner,
                            StartDate = b.StartDate,
                            EndDate = b.EndDate
                        };
                        return bDto;
                    }).ToList()
                }));

            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get available rooms for location: " + location, ex);
            }           
            return rooms;
        }

        [HttpGet]
        [Route("{location}/{start:DateTime}/{end:DateTime}/{numberOfPeople:int}/{smartRoom:bool}")]
        public List<RoomDTO> GetAvailableRoomsForLocation(string location, DateTime start, DateTime end, int numberOfPeople, bool smartRoom)
        {
            List<RoomDTO> rooms = new List<RoomDTO>();

            if (location == null)
                return new List<RoomDTO>();

            List<Room> roomData = new List<Room>();
            //var locationRooms = db.Rooms.Where(x => x.Location.Name == location && x.SeatCount >= numberOfPeople).ToList();

            try
            {
                var availableRooms = db.Rooms.Where(x => x.Location.Name == location && x.SeatCount >= numberOfPeople &&
                                               (x.Bookings.Where(b => start < b.EndDate && end > b.StartDate).Count() == 0)) //Do any bookings overlap
                                .OrderBy(r => r.SeatCount).ThenBy(r => r.SmartRoom)
                                .ToList();

                roomData.AddRange(availableRooms);

                roomData.ForEach(x => rooms.Add(new RoomDTO()
                {
                    ID = x.ID,
                    RoomName = x.RoomName,
                    PhoneCount = x.PhoneCount,
                    ComputerCount = x.ComputerCount,
                    SeatCount = x.SeatCount,
                    SmartRoom = x.SmartRoom,
                    Bookings = x.Bookings.Where(b => b.StartDate.Date == start.Date && b.EndDate.Date == end.Date).Select(b =>
                    {
                        BookingDTO bDto = new BookingDTO()
                        {
                            ID = b.ID,
                            Owner = b.Owner,
                            StartDate = b.StartDate,
                            EndDate = b.EndDate
                        };
                        return bDto;
                    }).ToList()
                }));
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get available rooms for location: " + location, ex);
            }
            return rooms;
        }

        [HttpGet]
        [Route("{location}/{start:DateTime}/{end:DateTime}/{numberOfPeople:int}/{smartRoom:bool}/{existingBookignId:int}")]
        public List<RoomDTO> GetAvailableRoomsForLocation(string location, DateTime start, DateTime end, int numberOfPeople, bool smartRoom, int existingBookignId)
        {
            List<RoomDTO> rooms = new List<RoomDTO>();

            if (location == null)
                return new List<RoomDTO>();

            List<Room> roomData = new List<Room>();
            //var locationRooms = db.Rooms.Where(x => x.Location.Name == location && x.SeatCount >= numberOfPeople).ToList();

            try
            {
                var availableRooms = db.Rooms.Where(x => x.Location.Name == location && x.SeatCount >= numberOfPeople &&
                                               (x.Bookings.Where(b => start < b.EndDate && end > b.StartDate && b.ID != existingBookignId).Count() == 0)) //Do any bookings overlap
                                .OrderBy(r => r.SeatCount).ThenBy(r => r.SmartRoom)
                                .ToList();

                roomData.AddRange(availableRooms);

                roomData.ForEach(x => rooms.Add(new RoomDTO()
                {
                    ID = x.ID,
                    RoomName = x.RoomName,
                    PhoneCount = x.PhoneCount,
                    ComputerCount = x.ComputerCount,
                    SeatCount = x.SeatCount,
                    SmartRoom = x.SmartRoom,
                    Bookings = x.Bookings.Where(b => b.StartDate.Date == start.Date && b.EndDate.Date == end.Date).Select(b =>
                    {
                        BookingDTO bDto = new BookingDTO()
                        {
                            ID = b.ID,
                            Owner = b.Owner,
                            StartDate = b.StartDate,
                            EndDate = b.EndDate
                        };
                        return bDto;
                    }).ToList()
                }));
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get available rooms for location: " + location, ex);
            }
            return rooms;
        }
    }
}
