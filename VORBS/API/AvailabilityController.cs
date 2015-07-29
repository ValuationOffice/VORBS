using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VORBS.Models;
using VORBS.DAL;
using VORBS.Models.DTOs;
using VORBS.Utils;
using System.Data.Entity.Core.Objects;

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
        [Route("{location}/{start:DateTime}/{smartRoom:bool}")]
        public List<RoomDTO> GetAllRoomBookingsForLocation(string location, DateTime start, bool smartRoom)
        {
            List<RoomDTO> rooms = new List<RoomDTO>();

            if (location == null)
                return new List<RoomDTO>();

            List<Room> roomData = new List<Room>();

            try
            {
                var availableRooms = db.Rooms.Where(x => x.Location.Name == location && x.Active == true)
                    .OrderBy(r => r.SeatCount)
                    .ToList();

                if (smartRoom)
                    availableRooms = availableRooms.Where(r => r.SmartRoom == true).ToList();

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
                            EndDate = b.EndDate,
                            IsSmartMeeting = b.IsSmartMeeting
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
                            EndDate = b.EndDate,
                            IsSmartMeeting = b.IsSmartMeeting
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
                            EndDate = b.EndDate,
                            IsSmartMeeting = b.IsSmartMeeting
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
                var availableRooms = db.Rooms.Where(x => x.Location.Name == location && x.SeatCount >= numberOfPeople && x.Active == true && x.SmartRoom == smartRoom &&
                                               (x.Bookings.Where(b => start < b.EndDate && end > b.StartDate).Count() == 0)) //Do any bookings overlap
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
                    Bookings = x.Bookings.Where(b => b.StartDate.Date == start.Date && b.EndDate.Date == end.Date).Select(b =>
                    {
                        BookingDTO bDto = new BookingDTO()
                        {
                            ID = b.ID,
                            Owner = b.Owner,
                            StartDate = b.StartDate,
                            EndDate = b.EndDate,
                            IsSmartMeeting = b.IsSmartMeeting
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
                var availableRooms = db.Rooms.Where(x => x.Location.Name == location && x.SeatCount >= numberOfPeople && x.Active == true && x.SmartRoom == smartRoom &&
                                               (x.Bookings.Where(b => start < b.EndDate && end > b.StartDate && b.ID != existingBookignId).Count() == 0)) //Do any bookings overlap
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
                    Bookings = x.Bookings.Where(b => b.StartDate.Date == start.Date && b.EndDate.Date == end.Date).Select(b =>
                    {
                        BookingDTO bDto = new BookingDTO()
                        {
                            ID = b.ID,
                            Owner = b.Owner,
                            StartDate = b.StartDate,
                            EndDate = b.EndDate,
                            IsSmartMeeting = b.IsSmartMeeting
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

        /// <summary>
        /// Finds meetings that clash with a given booking
        /// </summary>
        /// <param name="originalBooking">Bookings to check for clashes</param>
        /// <param name="clashedBooking">OUT Booking that possibly clashes with the booking</param>
        /// <returns>Boolean based on whether there was a clash.</returns>
        protected internal bool DoesMeetingClash(Booking originalBooking, out Booking clashedBooking)
        {
            var clashedBookings = db.Bookings
                //Meetings in the same room
                .Where(x => x.RoomID == originalBooking.RoomID && x.Room.LocationID == db.Rooms.FirstOrDefault(r => r.ID == x.RoomID).LocationID)
                //Where meeting clashes
                .Where(b => originalBooking.StartDate <= b.StartDate && originalBooking.EndDate >= b.StartDate)
                .ToList();

            clashedBooking = clashedBookings.FirstOrDefault();
            return clashedBookings.Count > 0;

        }

        /// <summary>
        /// Finds meetings that clash with a certain time on specific dates.
        /// </summary>
        /// <param name="room">Room to check for meetings in. Provided as this allows us to specify the room, rather than riskly grabbing the room of a random booking</param>
        /// <param name="startTime">Time for which the meetings will begin</param>
        /// <param name="endTime">Time for which the meetings will end</param>
        /// <param name="dates">Dates for to check for meeting clashes</param>
        /// <param name="clashedBookings">OUT bookings which clash with the given parameters</param>
        /// <returns>Boolean based on whether there were any clashes</returns>
        protected internal bool DoMeetingsClashRecurringly(Room room, TimeSpan startTime, TimeSpan endTime, DateTime date, out IEnumerable<Booking> clashedBookings)
        {
            ////Get dates only as we are checking against only the date portion in the DB
            //var datesOnly = dates.Select(x => x.Date);

            var totalBookingsClashed = db.Bookings
                //Bookings only for this room
                    .Where(x => x.RoomID == room.ID)
                //Only bookings on the certain days that we want to book for
                    .Where(y => date.Date == EntityFunctions.TruncateTime(y.StartDate))
                //Only bookings on the days that intersect our given time period
                    .Where(z => startTime <= EntityFunctions.CreateTime(z.StartDate.Hour, z.StartDate.Minute, z.StartDate.Second) && endTime >= EntityFunctions.CreateTime(z.StartDate.Hour, z.StartDate.Minute, z.StartDate.Second))
                    .ToList();

            clashedBookings = totalBookingsClashed;
            return totalBookingsClashed.Count > 0;
        }

        protected internal bool DoMeetingsClashRecurringly(List<Room> rooms, TimeSpan startTime, TimeSpan endTime, List<DateTime> dates, out List<Booking> allClashedBookings)
        {
            List<Booking> currentClashedBookings = new List<Booking>();
            IEnumerable<Booking> clashedBookings;

            bool clashed = false;
            int k = 0;

            for (int i = 0; i < rooms.Count; i++)
            {
                if (DoMeetingsClashRecurringly(rooms[i], startTime, endTime, dates[k], out clashedBookings))
                {
                    currentClashedBookings.AddRange(clashedBookings);
                    clashed = true;
                }

                k = (dates.Count() == (k + 1)) ? 0 : k + 1;
            }

            allClashedBookings = currentClashedBookings;
            return clashed;
        }


        protected internal Room GetAlternateRoom(TimeSpan startTime, TimeSpan endTime, int numberOfAttendees, int locationId, bool orderAsc)
        {
            var availableRooms = db.Rooms.Where(x => x.Location.ID == locationId && x.SeatCount >= numberOfAttendees && x.Active == true &&
                               (x.Bookings.Where(b => startTime < EntityFunctions.CreateTime(b.EndDate.Hour, b.EndDate.Minute, b.EndDate.Second) && endTime > EntityFunctions.CreateTime(b.StartDate.Hour, b.StartDate.Minute, b.StartDate.Second)).Count() == 0));


            availableRooms = (orderAsc) ? availableRooms.OrderBy(r => r.SeatCount) : availableRooms.OrderByDescending(r => r.SeatCount);

            return availableRooms.FirstOrDefault();
        }

        protected internal Room GetAlternateSmartRoom(int bookingRoomId, DateTime startDate, DateTime endDate, int locationId)
        {
            var availableRooms = db.Rooms.Where(x => x.Location.ID == locationId && x.SmartRoom == true && x.Active == true && x.ID != bookingRoomId &&
                                    (x.Bookings.Where(b => startDate <= b.StartDate && endDate >= b.StartDate)).Count() == 0)
                                 .OrderByDescending(r => r.SeatCount);

            return availableRooms.FirstOrDefault();
        }

        protected internal Room GetAlternateSmartRoom(IEnumerable<int> bookingRoomIds, DateTime startDate, DateTime endDate, int locationId)
        {
            var availableRooms = db.Rooms.Where(x => x.Location.ID == locationId && x.SmartRoom == true && x.Active == true && !bookingRoomIds.Contains(x.ID) &&
                                    (x.Bookings.Where(b => startDate <= b.StartDate && endDate >= b.StartDate)).Count() == 0)
                                 .OrderByDescending(r => r.SeatCount);

            return availableRooms.FirstOrDefault();
        }
    }
}
