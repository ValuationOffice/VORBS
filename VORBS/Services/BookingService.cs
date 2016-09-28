using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using VORBS.API;
using VORBS.DAL;
using VORBS.Models;

namespace VORBS.Services
{
    public class BookingService
    {
        private VORBSContext db;
        public BookingService(VORBSContext context)
        {
            db = context;
        }

        public List<Booking> GetByDateAndLocation(DateTime startDate, DateTime endDate, Location location)
        {
            List<Booking> bookings = db.Bookings
                    .Where(x => x.StartDate >= startDate && x.EndDate <= endDate && x.Room.Location.ID == location.ID)
                    .ToList();
            return bookings;
        }

        public List<Booking> GetByDateAndRoom(DateTime startDate, DateTime endDate, Room room)
        {
            List<Booking> bookings = db.Bookings
                   .Where(x => x.StartDate >= startDate && x.EndDate <= endDate && x.Room.ID == room.ID)
                   .ToList();
            return bookings;
        }

        public List<Booking> GetByDateRoomAndOwner(DateTime startDate, DateTime endDate, Room room, string owner)
        {
            List<Booking> bookings = db.Bookings
                   .Where(x => x.Owner == owner && x.StartDate >= startDate && x.EndDate <= endDate && x.Room.ID == room.ID)
                   .ToList();
            return bookings;
        }

        public List<Booking> GetByPartialDateRoomLocationSmartAndOwner(DateTime? startDate, DateTime? endDate, string owner, bool? smartRoom,  Room room, Location location)
        {
            //Building up a string of Lambda C# string (Using Linq.Dynamic) so we can on the fly change the where clause based on the concatanation
            string queryExpression = "";

            //Filter by owner
            queryExpression += (owner == null || owner.Equals("")) ? "" : (queryExpression.Length > 0) ? " AND OWNER = \"" + owner + "\"" : "OWNER = \"" + owner + "\"";

            // Filter by room
            //Standard pattern used all below, if the value is null to start, we wont filter on it if it isnt null, check if the string is empty already. If the string is not empty, start with the AND keyword as we are concatanating on otherwise start with your expression as it is the begging of the expression
            queryExpression += (room == null) ? "" : (queryExpression.Length > 0) ? " AND Room.ID = " + room.ID : "Room.ID = " + room.ID;


            queryExpression += (location == null) ? "" : (queryExpression.Length > 0) ? " AND Room.LocationID = " + location.ID : "Room.LocationID = " + location.ID;

            if (startDate != null)
            {
                //Need to get 00:00 and 23:59 times for the day as this allows us to get any booking during the day. We cant use .toshortdatestring etc inside a linq expression, even when not using dynamic linq
                DateTime startDate00Hours = DateTime.Parse(DateTime.Parse(startDate.ToString()).ToShortDateString());
                //Need to do a format string which looks like " DateTime(YEAR, MONTH, DAY, HOUR, MINUTE) " so that we can parse it in the expression and let Linq interperite it as a datetime creation
                string startDate00HoursFormatted = "DateTime(" + startDate00Hours.Year + ", " + startDate00Hours.Month + ", " + startDate00Hours.Day + ", " + startDate00Hours.Hour + ", " + startDate00Hours.Minute + ", " + startDate00Hours.Second + ")";
                DateTime startDate2359Hours = DateTime.Parse(DateTime.Parse(startDate.ToString()).ToShortDateString()).AddHours(23).AddMinutes(59).AddSeconds(59);
                string startDate2359HoursFormatted = "DateTime(" + startDate2359Hours.Year + ", " + startDate2359Hours.Month + ", " + startDate2359Hours.Day + ", " + startDate2359Hours.Hour + ", " + startDate2359Hours.Minute + ", " + startDate2359Hours.Second + ")";
                queryExpression += (startDate == null) ? "" : (queryExpression.Length > 0) ? " AND StartDate >= " + startDate00HoursFormatted + " AND EndDate <= " + startDate2359HoursFormatted + "" : " StartDate >= " + startDate00HoursFormatted + " AND EndDate <= " + startDate2359HoursFormatted;
            }

            if (endDate != null)
            {
                DateTime endDateValue = endDate.Value;
                string endDateFormatted = "DateTime(" + endDateValue.Year + ", " + endDateValue.Month + ", " + endDateValue.Day + ", " + endDateValue.Hour + ", " + endDateValue.Minute + ", " + endDateValue.Second + ")";
                //queryExpression += (startDate == null) ? "" : (queryExpression.Length > 0) ? " AND StartDate >= " + startDate00HoursFormatted + " AND EndDate <= " + startDate2359HoursFormatted + "" : " StartDate >= " + startDate00HoursFormatted + " AND EndDate <= " + startDate2359HoursFormatted;
                queryExpression += (queryExpression.Length > 0) ? " AND EndDate >= " + endDateFormatted : "EndDate >= " + endDateFormatted;
            }

            if (smartRoom != null)
            {
                queryExpression += (!smartRoom.Value) ? "" : (queryExpression.Length > 0) ? " AND IsSmartMeeting = " + smartRoom.Value : "IsSmartMeeting = " + smartRoom.Value;
            }
            

            var bookings = db.Bookings.Where(queryExpression).ToList();
            return bookings;
        }

        public List<Booking> GetByDateRoomAndPid(DateTime startDate, DateTime endDate, Room room, string pid)
        {
            List<Booking> bookings = db.Bookings
                   .Where(x => x.PID == pid && x.StartDate >= startDate && x.EndDate <= endDate && x.Room.ID == room.ID)
                   .ToList();
            return bookings;
        }

        public List<Booking> GetByPartialDateRoomSmartLocationAndPid(DateTime? startDate, DateTime? endDate, string Pid, bool? smartRoom, Room room, Location location)
        {
            //Building up a string of Lambda C# string (Using Linq.Dynamic) so we can on the fly change the where clause based on the concatanation
            string queryExpression = "";

            //Filter by owner
            queryExpression += (Pid == null || Pid.Equals("")) ? "" : (queryExpression.Length > 0) ? " AND PID = \"" + Pid + "\"" : "PID = \"" + Pid + "\"";

            // Filter by room
            //Standard pattern used all below, if the value is null to start, we wont filter on it if it isnt null, check if the string is empty already. If the string is not empty, start with the AND keyword as we are concatanating on otherwise start with your expression as it is the begging of the expression
            queryExpression += (room == null) ? "" : (queryExpression.Length > 0) ? " AND Room.ID = " + room.ID : "Room.ID = " + room.ID;

            queryExpression += (location == null) ? "" : (queryExpression.Length > 0) ? " AND Room.LocationID = " + location.ID : "Room.LocationID = " + location.ID;

            if (startDate != null)
            {
                //Need to get 00:00 and 23:59 times for the day as this allows us to get any booking during the day. We cant use .toshortdatestring etc inside a linq expression, even when not using dynamic linq
                DateTime startDate00Hours = DateTime.Parse(DateTime.Parse(startDate.ToString()).ToShortDateString());
                //Need to do a format string which looks like " DateTime(YEAR, MONTH, DAY, HOUR, MINUTE) " so that we can parse it in the expression and let Linq interperite it as a datetime creation
                string startDate00HoursFormatted = "DateTime(" + startDate00Hours.Year + ", " + startDate00Hours.Month + ", " + startDate00Hours.Day + ", " + startDate00Hours.Hour + ", " + startDate00Hours.Minute + ", " + startDate00Hours.Second + ")";
                DateTime startDate2359Hours = DateTime.Parse(DateTime.Parse(startDate.ToString()).ToShortDateString()).AddHours(23).AddMinutes(59).AddSeconds(59);
                string startDate2359HoursFormatted = "DateTime(" + startDate2359Hours.Year + ", " + startDate2359Hours.Month + ", " + startDate2359Hours.Day + ", " + startDate2359Hours.Hour + ", " + startDate2359Hours.Minute + ", " + startDate2359Hours.Second + ")";
                queryExpression += (startDate == null) ? "" : (queryExpression.Length > 0) ? " AND StartDate >= " + startDate00HoursFormatted + " AND EndDate <= " + startDate2359HoursFormatted + "" : " StartDate >= " + startDate00HoursFormatted + " AND EndDate <= " + startDate2359HoursFormatted;
            }

            if (endDate != null)
            {
                DateTime endDateValue = endDate.Value;
                string endDateFormatted = "DateTime(" + endDateValue.Year + ", " + endDateValue.Month + ", " + endDateValue.Day + ", " + endDateValue.Hour + ", " + endDateValue.Minute + ", " + endDateValue.Second + ")";
                //queryExpression += (startDate == null) ? "" : (queryExpression.Length > 0) ? " AND StartDate >= " + startDate00HoursFormatted + " AND EndDate <= " + startDate2359HoursFormatted + "" : " StartDate >= " + startDate00HoursFormatted + " AND EndDate <= " + startDate2359HoursFormatted;
                queryExpression += (queryExpression.Length > 0) ? " AND EndDate >= " + endDateFormatted : "EndDate >= " + endDateFormatted;
            }

            if (smartRoom != null)
            {
                queryExpression += (!smartRoom.Value) ? "" : (queryExpression.Length > 0) ? " AND IsSmartMeeting = " + smartRoom.Value : "IsSmartMeeting = " + smartRoom.Value;
            }

            var bookings = db.Bookings.Where(queryExpression).ToList();
            return bookings;
        }

        public List<Booking> GetByDateAndPidForPeriod(int period, string pid, DateTime startDate)
        {
            DateTime endDuration = startDate.AddDays(period - 1);
            endDuration = DateTime.Parse(endDuration.ToShortDateString()).AddHours(23).AddMinutes(59).AddSeconds(59);

            List<Booking> bookings = db.Bookings
                .Where(x => x.PID == pid && x.EndDate >= startDate && x.EndDate <= endDuration).ToList()
                .OrderBy(x => x.StartDate)
                .ToList();
            return bookings;
        }

        public List<Booking> GetByDateAndOwnerForPeriod(int period, string owner, DateTime startDate)
        {
            DateTime endDuration = startDate.AddDays(period - 1);
            endDuration = DateTime.Parse(endDuration.ToShortDateString()).AddHours(23).AddMinutes(59).AddSeconds(59);

            List<Booking> bookings = db.Bookings
                .Where(x => x.Owner == owner && x.EndDate >= startDate && x.EndDate <= endDuration).ToList()
                .OrderBy(x => x.StartDate)
                .ToList();
            return bookings;
        }

        public Booking GetById(int id)
        {
            Booking booking = db.Bookings.Single(b => b.ID == id);
            return booking;
        }

        public Booking UpdateExistingBooking(Booking existingBooking, Booking editBooking)
        {
            if (!HasBookingChanged(editBooking, existingBooking))
                throw new NoBookingChangesFoundException();

            editBooking.ID = existingBooking.ID;
            editBooking.Owner = existingBooking.Owner;
            editBooking.PID = existingBooking.PID;
            editBooking.Room = db.Rooms.SingleOrDefault(x => x.ID == editBooking.RoomID);

            db.Entry(existingBooking).CurrentValues.SetValues(editBooking);
            db.ExternalAttendees.RemoveRange(db.ExternalAttendees.Where(x => x.BookingID == editBooking.ID));

            if (editBooking.ExternalAttendees != null)
            {
                editBooking.ExternalAttendees.ToList().ForEach(x => x.BookingID = editBooking.ID);
                db.ExternalAttendees.AddRange(editBooking.ExternalAttendees);
            }

            db.SaveChanges(editBooking, false);

            return GetById(editBooking.ID);
        }

        public bool DeleteById(int Id)
        {
            try
            {
                Booking booking = db.Bookings.First(b => b.ID == Id);
                db.Bookings.Remove(booking);
                db.SaveChanges();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        

        protected internal bool HasBookingChanged(Booking editBooking, Booking existingBooking)
        {
            if (existingBooking.ExternalAttendees == null)
                existingBooking.ExternalAttendees = new List<ExternalAttendees>();
            if (editBooking.ExternalAttendees == null)
                editBooking.ExternalAttendees = new List<ExternalAttendees>();

            //Return true so we can handle the orginial error
            if (editBooking == null || existingBooking == null)
                return true;

            if (editBooking.StartDate == existingBooking.StartDate && editBooking.EndDate == existingBooking.EndDate &&
                editBooking.NumberOfAttendees == existingBooking.NumberOfAttendees && editBooking.Subject == existingBooking.Subject &&
                editBooking.Flipchart == existingBooking.Flipchart && editBooking.Projector == existingBooking.Projector &&
                editBooking.DssAssist == existingBooking.DssAssist)
            {
                //External Attendess had multiply possible values
                if ((editBooking.ExternalAttendees == null && existingBooking.ExternalAttendees.Count == 0))
                    return false;

                if (existingBooking.ExternalAttendees.Count() != editBooking.ExternalAttendees.Count())
                    return true;

                List<ExternalAttendees> existingAttendees = existingBooking.ExternalAttendees.ToList();
                List<ExternalAttendees> newAttendees = editBooking.ExternalAttendees.ToList();

                for (int i = 0; i < newAttendees.Count; i++)
                {
                    if (existingAttendees[i].FullName != newAttendees[i].FullName || existingAttendees[i].CompanyName != newAttendees[i].CompanyName ||
                        existingAttendees[i].PassRequired != newAttendees[i].PassRequired)
                    {
                        return true;
                    }
                }

                return false;
            }

            return true;
        }

        protected internal Booking GetBookingForRecurringDate(DateTime recurringDate, Booking newBooking)
        {
            TimeSpan startTime = new TimeSpan(newBooking.StartDate.Hour, newBooking.StartDate.Minute, newBooking.StartDate.Second);
            TimeSpan endTime = new TimeSpan(newBooking.EndDate.Hour, newBooking.EndDate.Minute, newBooking.EndDate.Second);

            DateTime startDate = new DateTime(recurringDate.Year, recurringDate.Month, recurringDate.Day);
            startDate = startDate + startTime;

            DateTime endDate = new DateTime(recurringDate.Year, recurringDate.Month, recurringDate.Day);
            endDate = endDate + endTime;

            return new Booking()
            {
                DssAssist = newBooking.DssAssist,
                ExternalAttendees = newBooking.ExternalAttendees,
                Flipchart = newBooking.Flipchart,
                NumberOfAttendees = newBooking.Room.SeatCount,
                Owner = newBooking.Owner,
                PID = newBooking.PID,
                Projector = newBooking.Projector,
                RoomID = newBooking.RoomID,
                Room = newBooking.Room,
                Subject = newBooking.Subject,
                StartDate = startDate,
                EndDate = endDate,
                IsSmartMeeting = newBooking.IsSmartMeeting
            };
        }

        protected internal List<Booking> GetBookingsForRecurringDates(List<DateTime> recurringDates, Booking newBooking)
        {
            List<Booking> bookingsToCreate = new List<Booking>();

            recurringDates.ForEach(x => bookingsToCreate.Add(GetBookingForRecurringDate(x, newBooking)));

            return bookingsToCreate;
        }

        List<Booking> GetAvailableSmartRoomBookings(Booking newBooking, out List<Booking> clashedBookings)
        {
            List<Booking> bookingsToCreate = new List<Booking>();
            List<Booking> clashedBs = new List<Booking>();
            List<int> smartRoomIds = new List<int>();

            smartRoomIds.Add(newBooking.RoomID);

            //TODO: THIS MUST BE CHANGED TO THE AVAILABILITY SERVICE WHEN IT IS MADE.
            AvailabilityController aC = new AvailabilityController();

            foreach (var smartLoc in newBooking.SmartLoactions)
            {
                int smartLocId = 0;
                int.TryParse(smartLoc, out smartLocId);

                Room smartLocData = db.Rooms.Where(x => x.ID == smartLocId).Select(x => x).ToList().FirstOrDefault();

                Booking doesClash;
                Room roomToBook = null;
                if (aC.DoesMeetingClash(new Booking() { StartDate = newBooking.StartDate, EndDate = newBooking.EndDate, RoomID = smartLocData.ID }, out doesClash))
                {
                    roomToBook = aC.GetAlternateSmartRoom(smartRoomIds, newBooking.StartDate, newBooking.EndDate, db.Locations.Single(l => l.Name == smartLocData.Location.Name).ID);
                }
                else
                {
                    roomToBook = smartLocData;
                }

                if (roomToBook == null || bookingsToCreate.Select(x => x.Room).Contains(roomToBook))
                {
                    clashedBs.Add(new Booking()
                    {
                        StartDate = newBooking.StartDate,
                        Owner = newBooking.Owner,
                        IsSmartMeeting = true,
                        Room = smartLocData
                    });
                }
                else
                {
                    bookingsToCreate.Add(new Booking()
                    {
                        DssAssist = newBooking.DssAssist,
                        ExternalAttendees = newBooking.ExternalAttendees,
                        Flipchart = newBooking.Flipchart,
                        NumberOfAttendees = newBooking.Room.SeatCount,
                        Owner = newBooking.Owner,
                        PID = newBooking.PID,
                        Projector = newBooking.Projector,
                        RoomID = roomToBook.ID,
                        Room = roomToBook,
                        Subject = newBooking.Subject,
                        StartDate = newBooking.StartDate,
                        EndDate = newBooking.EndDate,
                        IsSmartMeeting = true
                    });

                    smartRoomIds.Add(roomToBook.ID);
                }
            }

            clashedBookings = clashedBs;
            return bookingsToCreate;
        }
    }

    public class NoBookingChangesFoundException : Exception
    {
        public override string Message
        {
            get
            {
                return "No changes found between bookings.";
            }
        }
    }
}