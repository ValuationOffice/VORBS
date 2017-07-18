using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using VORBS.DAL.Repositories;
using VORBS.Models;
using VORBS.Models.DTOs;
using VORBS.Security;

namespace VORBS.Services
{
    public class BookingsService
    {
        private IBookingRepository _bookingsRepository;
        private IRoomRepository _roomsRepository;
        private ILocationRepository _locationsRepository;
        private AvailabilityService _availabilityService;

        private IDirectoryService _directoryService;

        private NLog.Logger _logger;

        public BookingsService(NLog.Logger logger, IBookingRepository bookingRepository, IRoomRepository roomRepository, ILocationRepository locationRepository, IDirectoryService directoryService)
        {
            _bookingsRepository = bookingRepository;
            _roomsRepository = roomRepository;
            _locationsRepository = locationRepository;

            _directoryService = directoryService;
            _availabilityService = new AvailabilityService(_bookingsRepository, _roomsRepository, _locationsRepository);

            _logger = logger;
        }

        public void SaveNewBooking(Booking newBooking, User user, NameValueCollection appSettings)
        {
            List<Booking> bookingsToCreate = new List<Booking>();
            List<Booking> clashedBookings = new List<Booking>();
            List<Booking> deletedBookings = new List<Booking>();

            Room bookingRoom = _roomsRepository.GetRoomById(newBooking.RoomID);
            newBooking.RoomID = newBooking.Room.ID;

            newBooking.Owner = user.FullName;
            newBooking.PID = user.PayId.Identity;

            if (newBooking.Recurrence.IsRecurring)
                ProcessRecurringBookings(user, ref newBooking, ref clashedBookings, ref bookingsToCreate, ref deletedBookings);
            else if (newBooking.SmartLoactions.Count() > 0 && newBooking.Room.SmartRoom)
                ProcessSmartLocations(ref newBooking, ref clashedBookings, ref bookingsToCreate);
            else
                bookingsToCreate.Add(newBooking);

            //Reset  Room as we dont want to create another room
            bookingsToCreate.ForEach(x => x.Room = null);

            //add the recurrence link id if needed
            int? nextRecurringId = _bookingsRepository.GetNextRecurrenceId();
            if (nextRecurringId != null && nextRecurringId > 0)
                bookingsToCreate.ForEach(x => x.RecurrenceId = nextRecurringId);

            _bookingsRepository.SaveNewBookings(bookingsToCreate);
            _logger.Info("Booking successfully created: " + newBooking.ID);


            if (!bool.Parse(appSettings["sendEmails"].ToString()))
                return;
            string fromEmail = appSettings["fromEmail"];
            
            if (deletedBookings.Count > 0)
                SendEmailToBookingsRemoved(fromEmail, deletedBookings, newBooking, clashedBookings);

            newBooking.Room = _roomsRepository.GetRoomById(newBooking.RoomID);

            Location bookingsLocation = _roomsRepository.GetRoomById(newBooking.RoomID).Location;

            if (newBooking.PID.ToUpper() != user.PayId.Identity.ToUpper())
                SendAdminEmailsForNewBooking(fromEmail, user, newBooking, bookingsToCreate);
            else
                SendPersonalEmailsForNewBooking(fromEmail, user, newBooking, bookingsToCreate);

            if (newBooking.Flipchart || newBooking.Projector)
                SendEmailToFacilitiesForNewBooking(newBooking, fromEmail, bookingsLocation);
            if (newBooking.ExternalAttendees != null && newBooking.ExternalAttendees.Count > 0)
                SendEmailToSecurityForNewBooking(fromEmail, newBooking, bookingsLocation);
            if (newBooking.DssAssist)
                SendEmailToDSSForNewBooking(fromEmail, newBooking, bookingsLocation);
        }

        private void ProcessSmartLocations(ref Booking newBooking, ref List<Booking> clashedBookings, ref List<Booking> bookingsToCreate)
        {
            var smartBookings = _bookingsRepository.GetAvailableSmartRoomBookings(newBooking, out clashedBookings);

            //No Rooms available;
            if (clashedBookings.Count() > 0)
                throw new ClashedBookingsException(clashedBookings);

            newBooking.IsSmartMeeting = true;

            bookingsToCreate.Add(newBooking);
            bookingsToCreate.AddRange(smartBookings);
        }

        private void ProcessRecurringBookings(User user, ref Booking newBooking, ref List<Booking> clashedBookings, ref List<Booking> bookingsToCreate, ref List<Booking> deletedBookings)
        {
            List<DateTime> recurringDates = new List<DateTime>();

            recurringDates = GetDatesForRecurrencePeriod(newBooking.StartDate, newBooking.Recurrence);

            if (newBooking.SmartLoactions.Count() > 0 && newBooking.Room.SmartRoom)
            {
                var smartBookings = _bookingsRepository.GetAvailableSmartRoomBookings(newBooking, out clashedBookings);

                //No Rooms available
                if (clashedBookings.Count() > 0)
                    throw new ClashedBookingsException(clashedBookings);

                newBooking.IsSmartMeeting = true;
                smartBookings.Add(newBooking);

                foreach (var smartBooking in smartBookings)
                    bookingsToCreate.AddRange(GetBookingsForRecurringDates(recurringDates, smartBooking));
            }
            else
                bookingsToCreate.AddRange(GetBookingsForRecurringDates(recurringDates, newBooking));

            bool doMeetingsClash = false;

            doMeetingsClash = _availabilityService.DoMeetingsClashRecurringly(bookingsToCreate.Select(x => x.Room).OrderBy(y => y.Location.ID).ToList(), TimeSpan.Parse(newBooking.StartDate.ToShortTimeString()), TimeSpan.Parse(newBooking.EndDate.ToShortTimeString()), recurringDates, out clashedBookings);

            if (doMeetingsClash)
            {
                if (newBooking.Recurrence.SkipClashes)
                {
                    List<Booking> clashedBookingsCopy = clashedBookings;
                    bookingsToCreate.RemoveAll(x => clashedBookingsCopy.Select(c => c.StartDate.ToShortDateString()).Contains(x.StartDate.ToShortDateString()));
                }

                else if (newBooking.Recurrence.AutoAlternateRoom)
                {
                    foreach (var cB in clashedBookings)
                    {
                        Room newRoom;
                        if (newBooking.SmartLoactions.Count() > 0 && newBooking.Room.SmartRoom) //TODO: Change when we introduce new validation check in UI
                        {
                            var unAvaliableRooms = bookingsToCreate.Where(y => cB.Room.LocationID == y.Room.LocationID && y.RoomID != cB.RoomID).Select(x => x.RoomID).Distinct();
                            newRoom = _availabilityService.GetAlternateSmartRoom(unAvaliableRooms, cB.StartDate, cB.EndDate, cB.Room.LocationID);
                        }
                        else
                        {
                            TimeSpan startTime = new TimeSpan(newBooking.StartDate.Hour, newBooking.StartDate.Minute, newBooking.StartDate.Second);
                            TimeSpan endTime = new TimeSpan(newBooking.EndDate.Hour, newBooking.EndDate.Minute, newBooking.EndDate.Second);

                            newRoom = _availabilityService.GetAlternateRoom(startTime, endTime, newBooking.Room.SeatCount, cB.Room.LocationID, true);
                        }

                        if (newRoom == null)
                            throw new ClashedBookingsException(new List<Booking>() { cB });

                        Booking newClashedBooking = bookingsToCreate.First(x => x.RoomID == cB.RoomID && cB.StartDate == x.StartDate && cB.EndDate == x.EndDate);

                        newClashedBooking.Room = newRoom;
                        newClashedBooking.RoomID = newRoom.ID;
                    }
                }
                else if (newBooking.Recurrence.AdminOverwrite)
                {
                    //checking they are still admin
                    if (VorbsAuthorise.IsUserAuthorised(user.PayId, 1))
                    {
                        try
                        {
                            List<int> ids = clashedBookings.Select(x => x.ID).ToList();
                            var entityClashedBookings = _bookingsRepository.GetById(ids);
                            _bookingsRepository.Delete(entityClashedBookings);
                            deletedBookings.AddRange(clashedBookings);
                        }
                        catch (Exception exn)
                        {
                            _logger.ErrorException(string.Format("Unable to overwrite bookings as admin. Old Bookings: {0}.", clashedBookings.Select(x => x.ID)), exn);
                        }
                    }
                    else
                        throw new UnauthorisedOverwriteException();
                }
                else
                    throw new BookingConflictException(clashedBookings);
            }
        }

        private void SendEmailToBookingsRemoved(string fromEmail, List<Booking> deletedBookings, Booking newBooking, List<Booking> clashedBookings)
        {
            foreach (string owner in deletedBookings.Select(x => x.PID).Distinct())
            {
                try
                {
                    string toEmail = _directoryService.GetUser(new User.Pid(owner)).EmailAddress;
                    string errorMessage = (newBooking.Recurrence.AdminOverwriteMessage.Trim().Length > 0) ? newBooking.Recurrence.AdminOverwriteMessage : "An admin has cancelled these bookings.";
                    string body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/AdminMultiCancelledBookingWithMessage.cshtml", new Models.ViewModels.AdminMultiCancelledBookingWithMessage(clashedBookings.Where(x => x.PID == owner), errorMessage));

                    Utils.EmailHelper.SendEmail(fromEmail, toEmail, "Meeting room booking(s) cancellation", body);
                }
                catch (Exception exn)
                {
                    _logger.ErrorException("Unable to send personal email for booking deletions by admin. Owner: " + owner, exn);
                }

            }
        }

        private void SendEmailToDSSForNewBooking(string fromEmail, Booking newBooking, Location bookingsLocation)
        {
            string dssEmail = bookingsLocation.LocationCredentials.Where(x => x.Department == LocationCredentials.DepartmentNames.dss.ToString()).Select(x => x.Email).FirstOrDefault();
            if (dssEmail != null)
            {
                try
                {
                    string body = "";
                    body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/DSSNewBooking.cshtml", newBooking);
                    Utils.EmailHelper.SendEmail(fromEmail, dssEmail, string.Format("SMART room set up support on {0}", newBooking.StartDate.ToShortDateString()), body);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Unable to send E-Mail to DSS for new booking: " + newBooking.ID, ex);
                }
            }
        }

        private void SendEmailToSecurityForNewBooking(string fromEmail, Booking newBooking, Location bookingsLocation)
        {

            string securityEmail = bookingsLocation.LocationCredentials.Where(x => x.Department == LocationCredentials.DepartmentNames.security.ToString()).Select(x => x.Email).FirstOrDefault();
            if (securityEmail != null)
            {
                try
                {
                    string body = "";
                    body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/SecurityNewBooking.cshtml", newBooking);
                    Utils.EmailHelper.SendEmail(fromEmail, securityEmail, string.Format("External guests notifcation for {0}", newBooking.StartDate.ToShortDateString()), body);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Unable to send E-Mail to security for new booking: " + newBooking.ID, ex);
                }
            }
            else
            {
                try
                {
                    string body = "";
                    string toEmail = _directoryService.GetUser(new User.Pid(newBooking.PID)).EmailAddress;
                    body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/SecurityNewBookingBookersCopy.cshtml", newBooking);
                    Utils.EmailHelper.SendEmail(fromEmail, toEmail, string.Format("External guests security information for {0}", newBooking.StartDate.ToShortDateString()), body);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Unable to send E-Mail to security for new booking: " + newBooking.ID, ex);
                }
            }
        }

        private void SendEmailToFacilitiesForNewBooking(Booking newBooking, string fromEmail, Location bookingsLocation)
        {

            string facilitiesEmail = bookingsLocation.LocationCredentials.Where(x => x.Department == LocationCredentials.DepartmentNames.facilities.ToString()).Select(x => x.Email).FirstOrDefault();
            if (facilitiesEmail != null)
            {
                try
                {
                    string body = "";
                    body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/FacilitiesNewBooking.cshtml", newBooking);
                    Utils.EmailHelper.SendEmail(fromEmail, facilitiesEmail, string.Format("Meeting room equipment booking on {0}", newBooking.StartDate.ToShortDateString()), body);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Unable to send E-Mail to facilities for new booking: " + newBooking.ID, ex);
                }
            }
        }

        private void SendAdminEmailsForNewBooking(string fromEmail, User user, Booking newBooking, List<Booking> bookingsToCreate)
        {
            try
            {
                string toEmail = _directoryService.GetUser(new User.Pid(newBooking.PID)).EmailAddress;

                string body = "";

                if (newBooking.IsSmartMeeting)
                    body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/AdminNewSmartBooking.cshtml", bookingsToCreate);
                else
                {
                    if (newBooking.Recurrence.IsRecurring)
                        body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/AdminNewRecurringBooking.cshtml", new Models.ViewModels.NewRecurringBookingWithMessage(newBooking, GetRecurrenceSentance(newBooking)));
                    else
                        body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/AdminNewBooking.cshtml", newBooking);
                }

                Utils.EmailHelper.SendEmail(fromEmail, toEmail, "Meeting room booking confirmation", body);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to send personal email for new booking: " + newBooking.ID, ex);
            }
        }

        private void SendPersonalEmailsForNewBooking(string fromEmail, User user, Booking newBooking, List<Booking> bookingsToCreate)
        {
            try
            {
                string toEmail = _directoryService.GetUser(new User.Pid(newBooking.PID)).EmailAddress;

                string body = "";

                if (newBooking.IsSmartMeeting)
                    bookingsToCreate = GetRoomForBookings(bookingsToCreate);
                else
                    newBooking.Room = _roomsRepository.GetRoomById(newBooking.RoomID);

                if (newBooking.IsSmartMeeting)
                    body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/NewSmartBooking.cshtml", bookingsToCreate);
                else
                {
                    if (newBooking.Recurrence.IsRecurring)
                        body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/NewRecurringBooking.cshtml", new Models.ViewModels.NewRecurringBookingWithMessage(newBooking, GetRecurrenceSentance(newBooking)));
                    else
                        body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/NewBooking.cshtml", newBooking);
                }

                Utils.EmailHelper.SendEmail(fromEmail, toEmail, "Meeting room booking confirmation", body);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to send personal email for new booking: " + newBooking.ID, ex);
            }
        }

        protected internal List<Booking> GetRoomForBookings(List<Booking> bookingsToCreate)
        {
            bookingsToCreate.ForEach(b => b.Room = _roomsRepository.GetRoomById(b.RoomID));
            return bookingsToCreate;
        }

        protected internal Booking GetBookingForRecurringDate(DateTime recurringDate, Booking newBooking)
        {
            TimeSpan startTime = new TimeSpan(newBooking.StartDate.Hour, newBooking.StartDate.Minute, newBooking.StartDate.Second);
            TimeSpan endTime = new TimeSpan(newBooking.EndDate.Hour, newBooking.EndDate.Minute, newBooking.EndDate.Second);

            DateTime startDate = new DateTime(recurringDate.Year, recurringDate.Month, recurringDate.Day);
            startDate = startDate + startTime;

            DateTime endDate = new DateTime(recurringDate.Year, recurringDate.Month, recurringDate.Day);
            endDate = endDate + endTime;

            Booking newBookingToReturn = new Booking()
            {
                DssAssist = newBooking.DssAssist,
                ExternalAttendees = new List<ExternalAttendees>() { },// newBooking.ExternalAttendees,
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

            foreach (ExternalAttendees attendee in newBooking.ExternalAttendees)
            {
                ExternalAttendees newAttendee = new ExternalAttendees()
                {
                    FullName = attendee.FullName,
                    BookingID = attendee.BookingID,
                    CompanyName = attendee.CompanyName,
                    PassRequired = attendee.PassRequired
                };
                newBookingToReturn.ExternalAttendees.Add(newAttendee);
            }
            return newBookingToReturn;
        }

        protected internal List<Booking> GetBookingsForRecurringDates(List<DateTime> recurringDates, Booking newBooking)
        {
            List<Booking> bookingsToCreate = new List<Booking>();

            recurringDates.ForEach(x => bookingsToCreate.Add(GetBookingForRecurringDate(x, newBooking)));

            return bookingsToCreate;
        }

        protected internal List<DateTime> GetDatesForRecurrencePeriod(DateTime startDate, RecurrenceDTO recurrenceDetails)
        {
            List<DateTime> recurringDates = new List<DateTime>();

            switch (recurrenceDetails.Frequency)
            {
                case "daily":
                    for (int i = 0; i <= ((recurrenceDetails.EndDate - startDate).Days + 1); i = i + (recurrenceDetails.DailyDayCount))
                    {
                        //exclude those days that are a weekend
                        if (!new int[2] { 6, 0 }.ToList().Contains(((int)startDate.AddDays(i).DayOfWeek)))
                        {
                            recurringDates.Add(startDate.AddDays(i));
                        }
                    }
                    break;
                case "weekly":
                    //Make a new copy of the date, as we may need to change it to get the next weekday matching users criteria
                    DateTime nextStartDay = startDate;
                    if ((int)startDate.DayOfWeek != recurrenceDetails.WeeklyDay)
                    {
                        int offset = recurrenceDetails.WeeklyDay - (int)nextStartDay.DayOfWeek;
                        if (offset < 0)
                            offset = 7 + offset;

                        nextStartDay = nextStartDay.AddDays(offset);

                        if (nextStartDay > recurrenceDetails.EndDate)
                            break;
                    }

                    for (int i = 0; i <= (((recurrenceDetails.EndDate - nextStartDay).Days / (7 * recurrenceDetails.WeeklyWeekCount)) + 1); i++)
                    {
                        DateTime nextBookingDate = nextStartDay.AddDays((7 * recurrenceDetails.WeeklyWeekCount) * i);
                        if (nextBookingDate.Date > recurrenceDetails.EndDate.Date)
                            break;
                        //exclude those days that are a weekend
                        if (!new int[2] { 6, 0 }.ToList().Contains((int)nextBookingDate.DayOfWeek))
                        {
                            recurringDates.Add(nextBookingDate);
                        }
                    }
                    break;
                case "monthly":

                    for (int i = 0; i <= (((recurrenceDetails.EndDate.Year - startDate.Year) * 12) + recurrenceDetails.EndDate.Month - startDate.Month); i++)
                    {
                        DateTime firstOfMonth = startDate.AddMonths(i * (recurrenceDetails.MonthlyMonthCount)).AddDays((-1 * (startDate.Day)) + 1);
                        DateTime nextOccurence = new DateTime();

                        if (recurrenceDetails.MonthlyMonthDayCount == 0)
                        {
                            DateTime lastDay = new DateTime(firstOfMonth.Year, firstOfMonth.Month, 1).AddMonths(1).AddDays(-1);
                            DayOfWeek lastDow = lastDay.DayOfWeek;

                            int diff = recurrenceDetails.MonthlyMonthDay - (int)lastDow;

                            if (diff > 0) diff -= 7;

                            nextOccurence = lastDay.AddDays(diff);
                        }
                        else
                        {
                            DateTime nextDayOccurence = firstOfMonth;

                            if ((int)firstOfMonth.DayOfWeek != recurrenceDetails.WeeklyDay)
                            {
                                int offset = recurrenceDetails.MonthlyMonthDay - (int)firstOfMonth.DayOfWeek;
                                if (offset < 0)
                                    offset = 7 + offset;

                                nextDayOccurence = firstOfMonth.AddDays(offset);
                            }

                            DateTime nextOccur = nextDayOccurence.AddDays(7 * (recurrenceDetails.MonthlyMonthDayCount - 1));

                            if (!new int[2] { 6, 0 }.ToList().Contains((int)nextOccur.DayOfWeek))
                            {
                                nextOccurence = nextOccur;
                            }
                        }
                        if (nextOccurence <= recurrenceDetails.EndDate && nextOccurence >= startDate)
                        {
                            recurringDates.Add(nextOccurence);
                        }
                    }
                    break;
            }
            return recurringDates;
        }

        protected static internal string GetRecurrenceSentance(Booking newBooking)
        {
            string recurrenceSentance = null;
            switch (newBooking.Recurrence.Frequency)
            {
                case "daily":
                    recurrenceSentance = string.Format("Booking occurs every {0} day(s) effective {1} until {2} from {3} to {4}",
                                            newBooking.Recurrence.DailyDayCount, newBooking.StartDate.ToShortDateString(), newBooking.Recurrence.EndDate.ToShortDateString(),
                                            newBooking.StartDate.ToShortTimeString(), newBooking.EndDate.ToShortTimeString());
                    break;
                case "weekly":
                    recurrenceSentance = string.Format("Booking occurs every {0} weeks(s) on {1} effective {2} until {3} from {4} to {5}",
                                            newBooking.Recurrence.WeeklyWeekCount, (DayOfWeek)newBooking.Recurrence.WeeklyDay, newBooking.StartDate.ToShortDateString(),
                                            newBooking.Recurrence.EndDate.ToShortDateString(), newBooking.StartDate.ToShortTimeString(), newBooking.EndDate.ToShortTimeString());
                    break;
                case "monthly":
                    string[] words = new string[] { "Last", "First", "Second", "Third", "Fourth" };
                    recurrenceSentance = string.Format("Booking occurs the {0} {1} of every {2} month(s) effective {3} until {4} from {5} to {6}",
                                            words[newBooking.Recurrence.MonthlyMonthDayCount], (DayOfWeek)newBooking.Recurrence.MonthlyMonthDay, newBooking.Recurrence.MonthlyMonthCount,
                                            newBooking.StartDate.ToShortDateString(), newBooking.Recurrence.EndDate.ToShortDateString(), newBooking.StartDate.ToShortTimeString(), newBooking.EndDate.ToShortTimeString());
                    break;
            }

            return recurrenceSentance;
        }


        public class ClashedBookingsException : Exception
        {
            public List<Booking> clashedBookings;

            public ClashedBookingsException(List<Booking> clashedBookings)
            {
                this.clashedBookings = clashedBookings;
            }
        }

        public class BookingConflictException : ClashedBookingsException
        {
            public BookingConflictException(List<Booking> clashedBookings) : base(clashedBookings) { }
        }

        public class UnauthorisedOverwriteException : Exception { }

    }
}