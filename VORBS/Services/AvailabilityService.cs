using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Web;
using VORBS.DAL.Repositories;
using VORBS.Models;

namespace VORBS.Services
{
    public class AvailabilityService
    {
        private IBookingRepository _bookingRepository;
        private IRoomRepository _roomRepository;
        private ILocationRepository _locationRepository;

        public AvailabilityService(IBookingRepository bookingRepository, IRoomRepository roomRepository, ILocationRepository locationRepository)
        {
            _bookingRepository = bookingRepository;
            _roomRepository = roomRepository;
            _locationRepository = locationRepository;
        }

        /// <summary>
        /// Finds meetings that clash with a given booking
        /// </summary>
        /// <param name="originalBooking">Bookings to check for clashes</param>
        /// <param name="clashedBooking">OUT Booking that possibly clashes with the booking</param>
        /// <returns>Boolean based on whether there was a clash.</returns>
        protected internal bool DoesMeetingClash(Booking originalBooking, out List<Booking> clashedBookings)
        {
            Room room = _roomRepository.GetRoomById(originalBooking.RoomID);
            clashedBookings = _bookingRepository.GetByDateAndRoom(originalBooking.StartDate, room)
                //Where meeting clashes
                .Where(b => (originalBooking.StartDate <= b.StartDate && originalBooking.EndDate > b.StartDate) || (originalBooking.StartDate >= b.StartDate && originalBooking.StartDate < b.EndDate))
                .ToList();

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
        bool DoMeetingsClashRecurringly(Room room, TimeSpan startTime, TimeSpan endTime, DateTime date, out IEnumerable<Booking> clashedBookings)
        {
            ////Get dates only as we are checking against only the date portion in the DB
            //var datesOnly = dates.Select(x => x.Date);

            var totalBookingsClashed = _bookingRepository.GetByDateOnlyAndRoom(date, room)
                    //Only bookings on the days that intersect our given time period
                    .Where(z => startTime <= EntityFunctions.CreateTime(z.StartDate.Hour, z.StartDate.Minute, z.StartDate.Second) && endTime > EntityFunctions.CreateTime(z.StartDate.Hour, z.StartDate.Minute, z.StartDate.Second)
                 || startTime >= EntityFunctions.CreateTime(z.StartDate.Hour, z.StartDate.Minute, z.StartDate.Second) && startTime < EntityFunctions.CreateTime(z.EndDate.Hour, z.EndDate.Minute, z.EndDate.Second))
                .ToList(); // bug fix

            clashedBookings = totalBookingsClashed; // WORKING
            return totalBookingsClashed.Count > 0;

        }

       public bool DoMeetingsClashRecurringly(List<Room> rooms, TimeSpan startTime, TimeSpan endTime, List<DateTime> dates, out List<Booking> allClashedBookings)
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
            Location location = _locationRepository.GetLocationById(locationId);
            var availableRooms = _roomRepository.GetByLocationAndSeatCount(location, numberOfAttendees)
                               .Where(x => x.Bookings.Where(b => startTime < EntityFunctions.CreateTime(b.EndDate.Hour, b.EndDate.Minute, b.EndDate.Second) && endTime > EntityFunctions.CreateTime(b.StartDate.Hour, b.StartDate.Minute, b.StartDate.Second)).Count() == 0);


            availableRooms = (orderAsc) ? availableRooms.OrderBy(r => r.SeatCount) : availableRooms.OrderByDescending(r => r.SeatCount);

            return availableRooms.FirstOrDefault();
        }

        protected internal Room GetAlternateSmartRoom(int bookingRoomId, DateTime startDate, DateTime endDate, int locationId)
        {
            Location location = _locationRepository.GetLocationById(locationId);
            var availableRooms =  _roomRepository.GetByLocationAndSmartRoom(location, true).Where(x => x.ID != bookingRoomId &&
                                    (x.Bookings.Where(b => startDate <= b.StartDate && endDate >= b.StartDate)).Count() == 0)
                                 .OrderByDescending(r => r.SeatCount);

            return availableRooms.FirstOrDefault();
        }

        protected internal Room GetAlternateSmartRoom(IEnumerable<int> bookingRoomIds, DateTime startDate, DateTime endDate, int locationId)
        {
            Location location = _locationRepository.GetLocationById(locationId);
            var availableRooms = _roomRepository.GetByLocationAndSmartRoom(location, true).Where( x=> !bookingRoomIds.Contains(x.ID) &&
                                    (x.Bookings.Where(b => startDate <= b.StartDate && endDate >= b.StartDate)).Count() == 0)
                                 .OrderByDescending(r => r.SeatCount);

            return availableRooms.FirstOrDefault();
        }
    }
}