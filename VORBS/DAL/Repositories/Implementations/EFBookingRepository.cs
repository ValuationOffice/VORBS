using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using VORBS.API;
using VORBS.DAL;
using VORBS.Models;
using VORBS.Services;
using VORBS.Utils;

namespace VORBS.DAL.Repositories
{
    public class EFBookingRepository : IBookingRepository
    {
        private VORBSContext db;

        private ILocationRepository _locationRepository;
        private IRoomRepository _roomRepository;

        private NLog.Logger _logger;
        public EFBookingRepository(VORBSContext context, ILocationRepository locationRepository, IRoomRepository roomRepository)
        {
            db = context;
            _logger = NLog.LogManager.GetCurrentClassLogger();

            _locationRepository = locationRepository;
            _roomRepository = roomRepository;

            _logger.Trace(LoggerHelper.InitializeClassMessage());
        }

        public List<Booking> GetByDateAndLocation(DateTime startDate, Location location)
        {
            List<Booking> bookings = db.Bookings
                    .Where(x => x.StartDate >= startDate && x.Room.Location.ID == location.ID)
                    .ToList();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(bookings, startDate, location));

            return bookings;
        }

        public List<Booking> GetByDateAndRoom(DateTime startDate, Room room)
        {
            List<Booking> bookings = db.Bookings
                    .Where(x => x.StartDate >= startDate && x.Room.ID == room.ID)
                    .ToList();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(bookings, startDate, room));

            return bookings;
        }

        public List<Booking> GetByDateOnlyAndRoom(DateTime dateOnly, Room room)
        {
            List<Booking> bookings = db.Bookings
                    //Bookings only for this room
                    .Where(x => x.RoomID == room.ID)
                    //Only bookings on the certain days that we want to book for
                    .Where(y => dateOnly.Date == EntityFunctions.TruncateTime(y.StartDate)).ToList();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(bookings, dateOnly, room));

            return bookings;
        }

        public List<Booking> GetByDateAndLocation(DateTime startDate, DateTime endDate, Location location)
        {
            List<Booking> bookings = db.Bookings
                    .Where(x => x.StartDate >= startDate && x.EndDate <= endDate && x.Room.Location.ID == location.ID)
                    .ToList();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(bookings, startDate, endDate, location));

            return bookings;
        }

        public List<Booking> GetByDateAndPid(DateTime startDate, string pid)
        {
            List<Booking> bookings = db.Bookings
                .Where(x => x.StartDate >= startDate && x.PID == pid)
                .ToList();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(bookings, startDate, pid));

            return bookings;
        }

        public List<Booking> GetByDateAndOwner(DateTime startDate, string owner)
        {
            List<Booking> bookings = db.Bookings
                .Where(x => x.StartDate >= startDate && x.Owner == owner)
                .ToList();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(bookings, startDate, owner));

            return bookings;
        }

        public List<Booking> GetByDateAndRoom(DateTime startDate, DateTime endDate, Room room)
        {
            List<Booking> bookings = db.Bookings
                   .Where(x => x.StartDate >= startDate && x.EndDate <= endDate && x.Room.ID == room.ID)
                   .ToList();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(bookings, startDate, endDate, room));

            return bookings;
        }

        public List<Booking> GetByDateRoomAndOwner(DateTime startDate, DateTime endDate, Room room, string owner)
        {
            List<Booking> bookings = db.Bookings
                   .Where(x => x.Owner == owner && x.StartDate >= startDate && x.EndDate <= endDate && x.Room.ID == room.ID)
                   .ToList();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(bookings, startDate, endDate, room, owner));

            return bookings;
        }

        public List<Booking> GetByPartialDateRoomLocationSmartAndOwner(DateTime? startDate, DateTime? endDate, string owner, bool? smartRoom, Room room, Location location)
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

            _logger.Debug($"Executing query: {queryExpression}");

            var bookings = db.Bookings.Where(queryExpression).ToList();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(bookings, startDate, endDate, owner, smartRoom, room, location));

            return bookings;
        }

        public List<Booking> GetByDateRoomAndPid(DateTime startDate, DateTime endDate, Room room, string pid)
        {
            List<Booking> bookings = db.Bookings
                   .Where(x => x.PID == pid && x.StartDate >= startDate && x.EndDate <= endDate && x.Room.ID == room.ID)
                   .ToList();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(bookings, startDate, endDate, room, pid));

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

            _logger.Debug($"Executing query: {queryExpression}");

            var bookings = db.Bookings.Where(queryExpression).ToList();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(bookings, startDate, endDate, Pid, smartRoom, room, location));

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

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(bookings, period, pid, startDate));

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

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(bookings, period, owner, startDate));

            return bookings;
        }

        public Booking GetById(int id)
        {
            Booking booking = db.Bookings.Single(b => b.ID == id);

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(booking, id));

            return booking;
        }

        public List<Booking> GetById(List<int> ids)
        {
            List<Booking> booking = db.Bookings.Where(x => ids.Contains(x.ID)).ToList();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(booking, ids));

            return booking;
        }

        public List<Booking> GetByOwner(string owner)
        {
            List<Booking> bookings = db.Bookings.Where(x => x.Owner == owner).ToList();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(bookings, owner));

            return bookings;
        }

        public List<string> GetDistinctListOfOwners()
        {
            List<string> owners = db.Bookings.Select(x => x.Owner).Distinct().ToList();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(owners));

            return owners;
        }

        public Booking UpdateExistingBooking(Booking existingBooking, Booking editBooking)
        {
            try
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
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, existingBooking, editBooking));
            }
            catch (Exception e)
            {
                _logger.Error($"Unable to edit existing booking: ID {existingBooking.ID}. An error occured: {e.Message}");
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(null, existingBooking, editBooking));
                return null;
            }


            Booking result = GetById(editBooking.ID);

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(result, existingBooking, editBooking));

            return result;
        }

        public List<Booking> UpdateExistingBookings(List<Booking> existingBookings, List<Booking> editBookings)
        {
            bool succeeded = true;
            List<Booking> bookingsToSave = new List<Booking>();
            foreach (Booking editBooking in editBookings)
            {
                Booking existingBooking = existingBookings.Where(x => x.ID == editBooking.ID).First();
                try
                {
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
                    bookingsToSave.Add(editBooking);
                    _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, existingBookings, editBookings));
                }
                catch (Exception e)
                {
                    succeeded = false;
                    _logger.Error($"Unable to delete booking(s): {String.Join(", ", editBookings.Select(x => x.ID))}. An error occured: {e.Message}");
                    break;
                }
            }
            if (succeeded)
            {
                try
                {
                    db.SaveChanges(bookingsToSave, false);
                    List<int> editBookingIds = editBookings.Select(x => x.ID).ToList();

                    List<Booking> bookings = db.Bookings.Where(x => editBookingIds.Contains(x.ID)).ToList();

                    _logger.Trace(LoggerHelper.ExecutedFunctionMessage(bookings, existingBookings, editBookings));

                    return bookings;
                }
                catch (BookingConflictException ex)
                {
                    _logger.Error($"Unable to edit existing booking(s): {String.Join(", ", editBookings.Select(x => x.ID))}. An error occured: {ex.Message}");
                    _logger.Trace(LoggerHelper.ExecutedFunctionMessage(ex, existingBookings, editBookings));
                    throw ex;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Unable to edit existing booking(s): {String.Join(", ", editBookings.Select(x => x.ID))}. An error occured: {ex.Message}");
                    _logger.Trace(LoggerHelper.ExecutedFunctionMessage(null, existingBookings, editBookings));
                    return null;
                }
            }
            else
            {
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(null, existingBookings, editBookings));
                return null;
            }

        }

        public void SaveNewBookings(List<Booking> bookings, bool checkForClash = true)
        {
            try
            {
                db.Bookings.AddRange(bookings);
                db.SaveChanges(bookings, !checkForClash);
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, bookings, checkForClash));
            }
            catch (Exception exn)
            {
                _logger.Error($"Unable to create new bookings.: {String.Join(", ", bookings.Select(x => x.ID))}. An error occured: {exn.Message}");
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(exn, bookings, checkForClash));
                throw exn;
            }
        }

        public bool DeleteById(int Id)
        {
            try
            {
                Booking booking = db.Bookings.First(b => b.ID == Id);
                db.Bookings.Remove(booking);
                db.SaveChanges();

                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(true, Id));

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"Unable to delete booking: ID {Id}. An error occured: {e.Message}");
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(false, Id));
                return false;
            }
        }

        public bool Delete(List<Booking> bookings)
        {
            bool succeeded = true;
            foreach (Booking booking in bookings)
            {
                try
                {
                    db.Bookings.Remove(booking);
                }
                catch (Exception e)
                {
                    succeeded = false;
                    _logger.Error($"Unable to delete booking: ID {booking.ID}. An error occured: {e.Message}");
                    break;
                }
            }
            if (succeeded)
            {
                try
                {
                    db.SaveChanges();
                    _logger.Trace(LoggerHelper.ExecutedFunctionMessage(true, bookings));
                    return true;
                }
                catch (Exception)
                {
                    _logger.Trace(LoggerHelper.ExecutedFunctionMessage(false, bookings));
                    return false;
                }
            }
            else
            {
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(false, bookings));
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
            {
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(true, editBooking, existingBooking));
                return true;
            }


            if (editBooking.StartDate == existingBooking.StartDate && editBooking.EndDate == existingBooking.EndDate &&
                editBooking.NumberOfAttendees == existingBooking.NumberOfAttendees && editBooking.Subject == existingBooking.Subject &&
                editBooking.Flipchart == existingBooking.Flipchart && editBooking.Projector == existingBooking.Projector &&
                editBooking.DssAssist == existingBooking.DssAssist)
            {
                //External Attendess had multiply possible values
                if ((editBooking.ExternalAttendees == null && existingBooking.ExternalAttendees.Count == 0))
                {
                    _logger.Trace(LoggerHelper.ExecutedFunctionMessage(false, editBooking, existingBooking));
                    return false;
                }


                if (existingBooking.ExternalAttendees.Count() != editBooking.ExternalAttendees.Count())
                {
                    _logger.Trace(LoggerHelper.ExecutedFunctionMessage(true, editBooking, existingBooking));
                    return true;
                }


                List<ExternalAttendees> existingAttendees = existingBooking.ExternalAttendees.ToList();
                List<ExternalAttendees> newAttendees = editBooking.ExternalAttendees.ToList();

                for (int i = 0; i < newAttendees.Count; i++)
                {
                    if (existingAttendees[i].FullName != newAttendees[i].FullName || existingAttendees[i].CompanyName != newAttendees[i].CompanyName ||
                        existingAttendees[i].PassRequired != newAttendees[i].PassRequired)
                    {
                        _logger.Trace(LoggerHelper.ExecutedFunctionMessage(true, editBooking, existingBooking));
                        return true;
                    }
                }
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(false, editBooking, existingBooking));
                return false;
            }
            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(true, editBooking, existingBooking));
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

            Booking booking = new Booking()
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

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(booking, recurringDate, newBooking));

            return booking;
        }

        protected internal List<Booking> GetBookingsForRecurringDates(List<DateTime> recurringDates, Booking newBooking)
        {
            List<Booking> bookingsToCreate = new List<Booking>();

            recurringDates.ForEach(x => bookingsToCreate.Add(GetBookingForRecurringDate(x, newBooking)));

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(bookingsToCreate, recurringDates, newBooking));

            return bookingsToCreate;
        }

        public List<Booking> GetAvailableSmartRoomBookings(Booking newBooking, out List<Booking> clashedBookings)
        {
            List<Booking> bookingsToCreate = new List<Booking>();
            List<Booking> clashedBs = new List<Booking>();
            List<int> smartRoomIds = new List<int>();

            smartRoomIds.Add(newBooking.RoomID);

            AvailabilityService aC = new AvailabilityService(_logger, this, _roomRepository, _locationRepository);

            foreach (var smartLoc in newBooking.SmartLoactions)
            {
                int smartLocId = 0;
                int.TryParse(smartLoc, out smartLocId);

                Room smartLocData = db.Rooms.Where(x => x.ID == smartLocId).Select(x => x).ToList().FirstOrDefault();

                List<Booking> doesClash;
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

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(bookingsToCreate, clashedBookings));

            return bookingsToCreate;
        }

        public List<Booking> GetBookingsInRecurrence(int recurrenceId, bool noTracking = false)
        {
            Func<Booking, bool> query = x => x.RecurrenceId == recurrenceId;

            List<Booking> result = noTracking ? db.Bookings.AsNoTracking().Where(query).ToList() : db.Bookings.Where(query).ToList();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(result, recurrenceId, noTracking));

            return result;
        }

        public int GetNextRecurrenceId()
        {
            var db2 = new VORBSContext();

            int? currentRecurringLinkId = db2.Bookings.Where(x => x.RecurrenceId != null).ToList().Select(x => x.RecurrenceId).Max();
            //nextRecurringLinkid = current +1 or 1 if null (no bookings)
            int nextRecurringLinkid = (currentRecurringLinkId == null) ? 1 : currentRecurringLinkId.Value + 1;

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(nextRecurringLinkid));

            return nextRecurringLinkid;
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