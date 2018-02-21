using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using VORBS.DAL.Repositories;
using VORBS.Models;
using VORBS.Models.DTOs;
using VORBS.Models.ViewModels;
using VORBS.Security;
using VORBS.Utils;
using static VORBS.Models.User;

namespace VORBS.Services
{
    public class BookingsService
    {
        private IBookingRepository _bookingsRepository;
        private IRoomRepository _roomsRepository;
        private ILocationRepository _locationsRepository;
        private AvailabilityService _availabilityService;

        private Utils.EmailHelper _emailHelper;

        private IDirectoryService _directoryService;

        private NLog.Logger _logger;

        public BookingsService(NLog.Logger logger, IBookingRepository bookingRepository, IRoomRepository roomRepository, ILocationRepository locationRepository, IDirectoryService directoryService, Utils.EmailHelper emailHelper)
        {
            _logger = logger;

            _bookingsRepository = bookingRepository;
            _roomsRepository = roomRepository;
            _locationsRepository = locationRepository;

            _directoryService = directoryService;
            _availabilityService = new AvailabilityService(_logger, _bookingsRepository, _roomsRepository, _locationsRepository);

            _emailHelper = emailHelper;

            _logger.Trace(LoggerHelper.InitializeClassMessage());
        }

        public void DeleteExistingBooking(Booking booking, bool? recurrence, User user, NameValueCollection appSettings)
        {
            int? recurringBookingId = null;
            if (recurrence.Value)
                recurringBookingId = booking.RecurrenceId;

            List<Booking> allBookings = (recurringBookingId == null) ? new List<Booking> { booking } : _bookingsRepository.GetBookingsInRecurrence(recurringBookingId.Value);

            bool success = _bookingsRepository.Delete(allBookings);
            if (!success)
                throw new DeletionException();
            else
                _logger.Info("Booking(s) successfully cancelled: " + String.Join(", ", allBookings.Select(x => x.ID)));

            if (!bool.Parse(appSettings["sendEmails"].ToString()))
            {
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, booking, recurrence, user, appSettings));
                _logger.Debug($"SendEmails setting is false or empty - Skipping email sending");
                return;
            }

            string fromEmail = appSettings["fromEmail"];

            Room locationRoom = _roomsRepository.GetRoomById(booking.RoomID);
            allBookings.ForEach(x => x.Room = locationRoom);

            if (booking.PID.ToUpper() != user.PayId.Identity.ToUpper())
                SendEmailToOwnerForAdminDelete(fromEmail, booking, user, allBookings);
            else
                SendEmailToOwnerForDelete(fromEmail, booking, user, allBookings);
            if (booking.Flipchart || booking.Projector)
                SendEmailToFacilitiesForDelete(fromEmail, booking, allBookings);
            if (booking.DssAssist)
                SendEmailToDSSForDelete(fromEmail, booking, allBookings);

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, booking, recurrence, user, appSettings));
        }

        public void EditExistingBooking(Booking existingBooking, Booking editBooking, bool? recurrence, User user, NameValueCollection appSettings)
        {
            int? recurringBookingId = null;
            if (recurrence.Value)
                recurringBookingId = existingBooking.RecurrenceId;

            List<Booking> allBookings = (recurringBookingId == null) ? new List<Booking> { existingBooking } : _bookingsRepository.GetBookingsInRecurrence(recurringBookingId.Value);
            List<Booking> originalBookings = (recurringBookingId == null) ? new List<Booking> { existingBooking } : _bookingsRepository.GetBookingsInRecurrence(recurringBookingId.Value, true);

            _logger.Debug($"AllBookings count: {allBookings.Count()}, OriginalBookings count: {originalBookings.Count()}");

            List<Booking> editBookings = new List<Booking>();

            for (int i = 0; i < allBookings.Count; i++)
            {
                Booking currentBooking = allBookings[i];
                currentBooking.DssAssist = editBooking.DssAssist;
                if (editBooking.ExternalAttendees != null && editBooking.ExternalAttendees.Count > 0)
                {
                    foreach (ExternalAttendees attendee in editBooking.ExternalAttendees)
                    {
                        currentBooking.ExternalAttendees.Add(new ExternalAttendees()
                        {
                            BookingID = currentBooking.ID,
                            FullName = attendee.FullName,
                            CompanyName = attendee.CompanyName,
                            PassRequired = attendee.PassRequired
                        });
                    }
                }

                currentBooking.Flipchart = editBooking.Flipchart;
                currentBooking.NumberOfAttendees = editBooking.NumberOfAttendees;
                currentBooking.StartDate = currentBooking.StartDate.Date + new TimeSpan(editBooking.StartDate.Hour, editBooking.StartDate.Minute, editBooking.StartDate.Second);
                currentBooking.EndDate = currentBooking.EndDate.Date + new TimeSpan(editBooking.EndDate.Hour, editBooking.EndDate.Minute, editBooking.EndDate.Second);
                currentBooking.Projector = editBooking.Projector;
                currentBooking.Subject = editBooking.Subject;

                editBookings.Add(currentBooking);
                _logger.Debug($"Added booking: {currentBooking.ID} to list of Editbookings");
            }

            var updatedBookings = _bookingsRepository.UpdateExistingBookings(allBookings, editBookings);
            if (updatedBookings == null)
            {
                _logger.Debug("UpdatedBookings from repository is null");
                _logger.Fatal("Unable to edit booking(s): " + editBooking.ID);

                UnableToEditBookingException exn = new UnableToEditBookingException();
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(exn, existingBooking, editBooking, recurrence, user, appSettings));
                throw exn;
            }
            else
                _logger.Info("Booking(s) successfully editted: " + String.Join(", ", editBookings.Select(x => x.ID)));

            if (!bool.Parse(appSettings["sendEmails"].ToString()))
            {
                _logger.Debug("SendEmails config is false or empty - Skipping email sending");
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, existingBooking, editBooking, recurrence, user, appSettings));
                return;
            }

            string fromEmail = appSettings["fromEmail"];

            if (
                editBookings.Select(x => (x.Flipchart != originalBookings.Where(y => y.ID == x.ID).First().Flipchart)).Count() > 0
                || editBookings.Select(x => (x.Projector != originalBookings.Where(y => y.ID == x.ID).First().Projector)).Count() > 0)
            {
                SendEmailToFacilitiesForEdit(fromEmail, editBooking, editBookings, originalBookings);
            }

            if (editBookings.Select(x => (x.DssAssist != originalBookings.Where(y => y.ID == x.ID).First().DssAssist)).Count() > 0)
                SendEmailToDSSForEdit(fromEmail, editBooking, editBookings, originalBookings);


            Func<Booking, Booking, bool> attendeeCompare = delegate (Booking newBooking, Booking currentBooking)
            {
                return !(new HashSet<string>(currentBooking.ExternalAttendees.Select(x => x.FullName)).SetEquals(newBooking.ExternalAttendees.Select(x => x.FullName)));
            };
            if (editBookings.Select(x => attendeeCompare(x, originalBookings.Where(y => y.ID == x.ID).First())).Count() > 0)
                SendEmailToSecurityForEdit(fromEmail, editBooking, editBookings, originalBookings);

            if (existingBooking.PID.ToUpper() != user.PayId.Identity.ToUpper())
                SendEmailToOwnerForAdminEdit(fromEmail, existingBooking, editBookings);
            else
                SendEmailToOwnerForEdit(fromEmail, existingBooking, editBookings, user);

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
            {
                ProcessRecurringBookings(user, ref newBooking, ref clashedBookings, ref bookingsToCreate, ref deletedBookings);

                int? nextRecurringId = _bookingsRepository.GetNextRecurrenceId();
                if (nextRecurringId != null && nextRecurringId > 0)
                    bookingsToCreate.ForEach(x => x.RecurrenceId = nextRecurringId);
            }
            else if (newBooking.SmartLoactions.Count() > 0 && newBooking.Room.SmartRoom)
                ProcessSmartLocations(ref newBooking, ref clashedBookings, ref bookingsToCreate);
            else
                ProcessSingularBooking(ref newBooking, ref clashedBookings, ref bookingsToCreate);

            //Reset  Room as we dont want to create another room
            bookingsToCreate.ForEach(x => x.Room = null);

            _bookingsRepository.SaveNewBookings(bookingsToCreate);
            _logger.Info("Booking successfully created: " + newBooking.ID);


            if (!bool.Parse(appSettings["sendEmails"].ToString()))
            {
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, newBooking, user, appSettings));
                return;
            }

            string fromEmail = appSettings["fromEmail"];

            if (deletedBookings.Count > 0)
                SendEmailToOwnerForAdminOverwriteWithMessage(fromEmail, deletedBookings, newBooking, clashedBookings);

            newBooking.Room = _roomsRepository.GetRoomById(newBooking.RoomID);

            Location bookingsLocation = _roomsRepository.GetRoomById(newBooking.RoomID).Location;

            if (newBooking.PID.ToUpper() != user.PayId.Identity.ToUpper())
                SendEmailToOwnerForAdminCreate(fromEmail, user, newBooking, bookingsToCreate);
            else
                SendEmailToOwnerForCreate(fromEmail, user, newBooking, bookingsToCreate);

            if (newBooking.Flipchart || newBooking.Projector)
                SendEmailToFacilitiesForCreate(newBooking, fromEmail, bookingsLocation);
            if (newBooking.ExternalAttendees != null && newBooking.ExternalAttendees.Count > 0)
                SendEmailToSecurityForCreate(fromEmail, newBooking, bookingsLocation);
            if (newBooking.DssAssist)
                SendEmailToDSSForCreate(fromEmail, newBooking, bookingsLocation);

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, newBooking, user, appSettings));
        }

        private void ProcessSingularBooking(ref Booking newBooking, ref List<Booking> clashedBookings, ref List<Booking> bookingsToCreate)
        {
            bool clashed = _availabilityService.DoesMeetingClash(newBooking, out clashedBookings);

            if (clashed)
            {
                _logger.Debug("Booking clashed with an existing booking");
                throw new ClashedBookingsException(clashedBookings);
            }

            bookingsToCreate.Add(newBooking);
            _logger.Debug($"Adding {newBooking} to list of bookings to create");
            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, newBooking, clashedBookings, bookingsToCreate));
        }

        private void ProcessSmartLocations(ref Booking newBooking, ref List<Booking> clashedBookings, ref List<Booking> bookingsToCreate)
        {
            var smartBookings = _bookingsRepository.GetAvailableSmartRoomBookings(newBooking, out clashedBookings);

            //No Rooms available;
            if (clashedBookings.Count() > 0)
            {
                _logger.Debug("Booking clashed with an existing booking");
                throw new ClashedBookingsException(clashedBookings);
            }

            newBooking.IsSmartMeeting = true;

            bookingsToCreate.Add(newBooking);
            bookingsToCreate.AddRange(smartBookings);

            _logger.Debug($"Adding {newBooking} to list of bookings to create");
            string ids = "";
            foreach (Booking smartBooking in smartBookings)
            {
                ids += smartBooking.ID + ", ";
            }
            _logger.Debug($"Adding {smartBookings.Count} smart bookings to list of bookings to create: IDs:  {ids}");
            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, newBooking, clashedBookings, bookingsToCreate));
        }

        private void ProcessRecurringBookings(User user, ref Booking newBooking, ref List<Booking> clashedBookings, ref List<Booking> bookingsToCreate, ref List<Booking> deletedBookings)
        {
            List<DateTime> recurringDates = new List<DateTime>();

            recurringDates = GetDatesForRecurrencePeriod(newBooking.StartDate, newBooking.Recurrence);

            if (newBooking.SmartLoactions.Count() > 0 && newBooking.Room.SmartRoom)
            {
                var smartBookings = _bookingsRepository.GetAvailableSmartRoomBookings(newBooking, out clashedBookings);

                if (clashedBookings.Count() > 0)
                {
                    ClashedBookingsException exn = new ClashedBookingsException(clashedBookings);
                    _logger.Trace(LoggerHelper.ExecutedFunctionMessage(exn, user, newBooking, clashedBookings, bookingsToCreate, deletedBookings));
                    throw exn;
                }

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
                _logger.Debug($"Meetings clash");

                if (newBooking.Recurrence.SkipClashes)
                {
                    List<Booking> clashedBookingsCopy = clashedBookings;
                    bookingsToCreate.RemoveAll(x => clashedBookingsCopy.Select(c => c.StartDate.ToShortDateString()).Contains(x.StartDate.ToShortDateString()));
                    _logger.Debug("Skipping clashes");
                }

                else if (newBooking.Recurrence.AutoAlternateRoom)
                {
                    foreach (var cB in clashedBookings)
                    {
                        Room newRoom;
                        if (newBooking.SmartLoactions.Count() > 0 && newBooking.Room.SmartRoom)
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
                        {
                            ClashedBookingsException exn = new ClashedBookingsException(new List<Booking>() { cB });

                            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(exn, user, newBooking, clashedBookings, bookingsToCreate, deletedBookings));

                            throw exn;
                        }


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
                        _logger.Debug("Overwriting messages");
                        try
                        {
                            List<int> ids = clashedBookings.Select(x => x.ID).ToList();
                            var entityClashedBookings = _bookingsRepository.GetById(ids);
                            _bookingsRepository.Delete(entityClashedBookings);
                            deletedBookings.AddRange(clashedBookings);

                            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, user, newBooking, clashedBookings, bookingsToCreate, deletedBookings));
                        }
                        catch (Exception exn)
                        {
                            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(exn, user, newBooking, clashedBookings, bookingsToCreate, deletedBookings));
                            _logger.ErrorException(string.Format("Unable to overwrite bookings as admin. Old Bookings: {0}.", clashedBookings.Select(x => x.ID)), exn);
                        }
                    }
                    else
                    {
                        UnauthorisedOverwriteException exn = new UnauthorisedOverwriteException();

                        _logger.Trace(LoggerHelper.ExecutedFunctionMessage(exn, user, newBooking, clashedBookings, bookingsToCreate, deletedBookings));

                        throw exn;
                    }

                }
                else
                {
                    BookingConflictException exn = new BookingConflictException(clashedBookings);

                    _logger.Trace(LoggerHelper.ExecutedFunctionMessage(exn, user, newBooking, clashedBookings, bookingsToCreate, deletedBookings));

                    throw exn;
                }
            }
        }

        private void SendEmailToOwnerForAdminDelete(string fromEmail, Booking booking, User user, List<Booking> allBookings)
        {
            try
            {
                string toEmail = _directoryService.GetUser(new User.Pid(booking.PID)).EmailAddress;
                string body = _emailHelper.GetEmailMarkup("~/Views/EmailTemplates/AdminCancelledBooking.cshtml", allBookings);

                if (!string.IsNullOrEmpty(body) && !string.IsNullOrEmpty(toEmail))
                {
                    _emailHelper.SendEmail(fromEmail, toEmail, "Meeting room booking cancellation(s)", body);
                    _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, fromEmail, booking, user, allBookings));
                }
                else
                    throw new Exception("Body or To-Email is null.");

            }
            catch (Exception ex)
            {
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(ex, fromEmail, booking, user, allBookings));
                _logger.ErrorException("Unable to send personal email for deleting booking(s): " + String.Join(", ", allBookings.Select(x => x.ID)), ex);
            }
        }

        private void SendEmailToOwnerForDelete(string fromEmail, Booking booking, User user, List<Booking> allBookings)
        {
            try
            {
                string toEmail = _directoryService.GetUser(new User.Pid(booking.PID)).EmailAddress;
                string body = _emailHelper.GetEmailMarkup("~/Views/EmailTemplates/CancelledBooking.cshtml", allBookings);

                if (!string.IsNullOrEmpty(body) && !string.IsNullOrEmpty(toEmail))
                {
                    _emailHelper.SendEmail(fromEmail, toEmail, "Meeting room booking cancellation(s)", body);
                    _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, fromEmail, booking, user, allBookings));
                }

                else
                    throw new Exception("Body or To-Email is null.");
            }
            catch (Exception ex)
            {
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(ex, fromEmail, booking, user, allBookings));
                _logger.ErrorException("Unable to send personal email for deleting booking(s): " + String.Join(", ", allBookings.Select(x => x.ID)), ex);
            }
        }

        private void SendEmailToFacilitiesForDelete(string fromEmail, Booking booking, List<Booking> allBookings)
        {
            Location bookingsLocation = _roomsRepository.GetRoomById(booking.RoomID).Location;

            try
            {
                string toEmail = bookingsLocation.LocationCredentials.Where(x => x.Department == LocationCredentials.DepartmentNames.facilities.ToString()).Select(x => x.Email).FirstOrDefault();
                string body = _emailHelper.GetEmailMarkup("~/Views/EmailTemplates/FacilitiesDeletedBooking.cshtml", allBookings);

                if (!string.IsNullOrEmpty(body) && !string.IsNullOrEmpty(toEmail))
                {
                    _emailHelper.SendEmail(fromEmail, toEmail, "Meeting room equipment cancellation for booking(s)", body);
                    _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, fromEmail, booking, allBookings));
                }

                else
                    throw new Exception("Body or To-Email is null.");
            }
            catch (Exception ex)
            {
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(ex, fromEmail, booking, allBookings));
                _logger.ErrorException("Unable to send email to facilities for deleting booking(s): " + String.Join(", ", allBookings.Select(x => x.ID)), ex);
            }
        }

        private void SendEmailToDSSForDelete(string fromEmail, Booking booking, List<Booking> allBookings)
        {
            Location bookingsLocation = _roomsRepository.GetRoomById(booking.RoomID).Location;

            try
            {
                string toEmail = bookingsLocation.LocationCredentials.Where(x => x.Department == LocationCredentials.DepartmentNames.dss.ToString()).Select(x => x.Email).FirstOrDefault();
                string body = _emailHelper.GetEmailMarkup("~/Views/EmailTemplates/DSSDeletedBooking.cshtml", allBookings);

                if (!string.IsNullOrEmpty(body) && !string.IsNullOrEmpty(toEmail))
                {
                    _emailHelper.SendEmail(fromEmail, toEmail, "Meeting room booking cancellation(s)", body);
                    _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, fromEmail, booking, allBookings));
                }

                else
                    throw new Exception("Body or To-Email is null.");
            }
            catch (Exception ex)
            {
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(ex, fromEmail, booking, allBookings));
                _logger.ErrorException("Unable to send email to dss for deleting booking(s): " + String.Join(", ", allBookings.Select(x => x.ID)), ex);
            }
        }

        private void SendEmailToOwnerForAdminEdit(string fromEmail, Booking existingBooking, List<Booking> editBookings)
        {
            try
            {
                string toEmail = _directoryService.GetUser(new Pid(existingBooking.PID)).EmailAddress;
                string body = _emailHelper.GetEmailMarkup("~/Views/EmailTemplates/AdminEdittedBooking.cshtml", editBookings);
                string subject = "Meeting room edit confirmation";

                if (!string.IsNullOrEmpty(body) && !string.IsNullOrEmpty(toEmail))
                {
                    _emailHelper.SendEmail(fromEmail, toEmail, subject, body);
                    _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, fromEmail, existingBooking, editBookings));
                }

                else
                    throw new Exception("Body or To-Email is null.");
            }
            catch (Exception ex)
            {
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(ex, fromEmail, existingBooking, editBookings));
                _logger.ErrorException("Unable to send personal email for editting booking(s): " + String.Join(", ", editBookings.Select(x => x.ID)), ex);
            }
        }

        private void SendEmailToOwnerForEdit(string fromEmail, Booking existingBooking, List<Booking> editBookings, User user)
        {
            Pid currentUserPid = user.PayId;

            try
            {
                string toEmail = _directoryService.GetUser(new Pid(existingBooking.PID)).EmailAddress;
                string body = _emailHelper.GetEmailMarkup("~/Views/EmailTemplates/EdittedBooking.cshtml", editBookings);
                string subject = "Meeting room booking confirmation";

                if (!string.IsNullOrEmpty(body) && !string.IsNullOrEmpty(toEmail))
                {
                    _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, fromEmail, existingBooking, editBookings, user));
                    _emailHelper.SendEmail(fromEmail, toEmail, subject, body);
                }

                else
                    throw new Exception("Body or To-Email is null.");
            }
            catch (Exception ex)
            {
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(ex, fromEmail, existingBooking, editBookings, user));
                _logger.ErrorException("Unable to send personal email for editting booking(s): " + String.Join(", ", editBookings.Select(x => x.ID)), ex);
            }
        }

        private void SendEmailToSecurityForEdit(string fromEmail, Booking editBooking, List<Booking> editBookings, List<Booking> originalBookings)
        {
            List<SecurityEdittedBooking> securityViewModels = new List<SecurityEdittedBooking>();
            Location bookingsLocation = _roomsRepository.GetRoomById(editBooking.RoomID).Location;

            foreach (Booking booking in editBookings)
            {
                Booking currentBooking = originalBookings.Where(x => x.ID == booking.ID).First();

                if (!(new HashSet<string>(currentBooking.ExternalAttendees.Select(x => x.FullName)).SetEquals(booking.ExternalAttendees.Select(x => x.FullName))))
                {

                    IEnumerable<ExternalAttendees> removedAttendees = currentBooking.ExternalAttendees.Where(x => !booking.ExternalAttendees.Select(y => y.FullName).Contains(x.FullName));
                    IEnumerable<ExternalAttendees> addedAttendees = booking.ExternalAttendees.Where(x => !currentBooking.ExternalAttendees.Select(y => y.FullName).Contains(x.FullName));
                    SecurityEdittedBooking viewModel = new Models.ViewModels.SecurityEdittedBooking() { EdittedBooking = booking, NewAttendees = addedAttendees, RemovedAttendees = removedAttendees };

                    securityViewModels.Add(viewModel);
                }
            }
            try
            {
                if (securityViewModels.Count > 0)
                {
                    string toEmail = bookingsLocation.LocationCredentials.Where(x => x.Department == LocationCredentials.DepartmentNames.security.ToString()).Select(x => x.Email).FirstOrDefault();
                    string body = _emailHelper.GetEmailMarkup("~/Views/EmailTemplates/SecurityEdittedBooking.cshtml", securityViewModels);
                    if (!string.IsNullOrEmpty(body) && !string.IsNullOrEmpty(toEmail))
                    {
                        _emailHelper.SendEmail(fromEmail, toEmail, "(Updated) External guests notification", body);
                        _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, fromEmail, editBooking, editBookings, originalBookings));
                    }
                    else
                        throw new Exception("Body or To-Email is null.");

                }
            }
            catch (Exception ex)
            {
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(ex, fromEmail, editBooking, editBookings, originalBookings));
                _logger.ErrorException("Unable to retrieve security email markup for editting booking(s): " + String.Join(", ", editBookings.Select(x => x.ID)), ex);
            }
        }

        private void SendEmailToDSSForEdit(string fromEmail, Booking editBooking, List<Booking> editBookings, List<Booking> originalBookings)
        {
            List<Booking> dssEditBookings = new List<Booking>();
            Location bookingsLocation = _roomsRepository.GetRoomById(editBooking.RoomID).Location;

            foreach (Booking booking in editBookings)
            {
                Booking currentBooking = originalBookings.Where(x => x.ID == booking.ID).First();
                if (booking.DssAssist != currentBooking.DssAssist)
                {
                    booking.Room.Location = currentBooking.Room.Location;
                    dssEditBookings.Add(booking);
                }
            }
            try
            {
                if (dssEditBookings.Count > 0)
                {
                    string toEmail = bookingsLocation.LocationCredentials.Where(x => x.Department == LocationCredentials.DepartmentNames.dss.ToString()).Select(x => x.Email).FirstOrDefault();
                    string body = _emailHelper.GetEmailMarkup("~/Views/EmailTemplates/DSSEdittedBooking.cshtml", dssEditBookings);

                    if (!string.IsNullOrEmpty(body) && !string.IsNullOrEmpty(toEmail))
                    {
                        _emailHelper.SendEmail(fromEmail, toEmail, "(Updated) SMART room set up support", body);
                        _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, fromEmail, editBooking, editBookings, originalBookings));
                    }
                    else
                        throw new Exception("Body or To-Email is null.");
                }
            }
            catch (Exception ex)
            {
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(ex, fromEmail, editBooking, editBookings, originalBookings));
                _logger.ErrorException("Unable to retrieve E-Mail markup for DSS for new booking(s): " + String.Join(", ", editBookings.Select(x => x.ID)), ex);
            }
        }

        private void SendEmailToFacilitiesForEdit(string fromEmail, Booking editBooking, List<Booking> editBookings, List<Booking> originalBookings)
        {
            Location bookingsLocation = _roomsRepository.GetRoomById(editBooking.RoomID).Location;

            List<FacilitiesEdittedBooking> facilitiesViewModels = new List<FacilitiesEdittedBooking>();

            foreach (Booking booking in editBookings)
            {
                Booking currentBooking = originalBookings.Where(x => x.ID == booking.ID).First();
                if ((currentBooking.Flipchart != booking.Flipchart) || (booking.Projector != currentBooking.Projector))
                {

                    List<string> addedEquipment = new List<string>();
                    List<string> removedEquipment = new List<string>();
                    if (booking.Projector != currentBooking.Projector)
                    {
                        //If it is present in edit booking, and above equality check is false, then we know the projector request is new
                        if (booking.Projector)
                            addedEquipment.Add("Projector");
                        else
                            removedEquipment.Add("Projector");
                    }
                    if (booking.Flipchart != currentBooking.Flipchart)
                    {
                        if (booking.Flipchart)
                            addedEquipment.Add("Flip Chart");
                        else
                            removedEquipment.Add("Flip Chart");
                    }

                    Models.ViewModels.FacilitiesEdittedBooking viewModel = new Models.ViewModels.FacilitiesEdittedBooking() { EdittedBooking = booking, OriginalBooking = currentBooking, EquipmentAdded = addedEquipment, EquipmentRemoved = removedEquipment };
                    facilitiesViewModels.Add(viewModel);
                }
            }
            try
            {
                if (facilitiesViewModels.Count > 0)
                {
                    string toEmail = bookingsLocation.LocationCredentials.Where(x => x.Department == LocationCredentials.DepartmentNames.facilities.ToString()).Select(x => x.Email).FirstOrDefault();
                    string body = _emailHelper.GetEmailMarkup("~/Views/EmailTemplates/FacilitiesEdittedBooking.cshtml", facilitiesViewModels);
                    if (!string.IsNullOrEmpty(body) && !string.IsNullOrEmpty(toEmail))
                    {
                        _emailHelper.SendEmail(fromEmail, toEmail, "(Updated) Meeting room equipment for booking(s)", body);
                        _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, fromEmail, editBooking, editBookings, originalBookings));
                    }
                    else
                        throw new Exception("Body or To-Email is null.");
                }
            }
            catch (Exception ex)
            {
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(ex, fromEmail, editBooking, editBookings, originalBookings));
                _logger.ErrorException("Unable to retrieve email markup for facilities for editted booking(s): " + String.Join(", ", editBookings.Select(x => x.ID)), ex);
            }
        }

        private void SendEmailToOwnerForAdminOverwriteWithMessage(string fromEmail, List<Booking> deletedBookings, Booking newBooking, List<Booking> clashedBookings)
        {
            foreach (string owner in deletedBookings.Select(x => x.PID).Distinct())
            {
                try
                {
                    string toEmail = _directoryService.GetUser(new User.Pid(owner)).EmailAddress;
                    string errorMessage = (newBooking.Recurrence.AdminOverwriteMessage.Trim().Length > 0) ? newBooking.Recurrence.AdminOverwriteMessage : "An admin has cancelled these bookings.";
                    string body = _emailHelper.GetEmailMarkup("~/Views/EmailTemplates/AdminMultiCancelledBookingWithMessage.cshtml", new Models.ViewModels.AdminMultiCancelledBookingWithMessage(clashedBookings.Where(x => x.PID == owner), errorMessage));

                    if (!string.IsNullOrEmpty(body) && !string.IsNullOrEmpty(toEmail))
                    {
                        _emailHelper.SendEmail(fromEmail, toEmail, "Meeting room booking(s) cancellation", body);
                        _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, fromEmail, deletedBookings, newBooking, clashedBookings));
                    }
                    else
                        throw new Exception("Body or To-Email is null.");
                }
                catch (Exception ex)
                {
                    _logger.Trace(LoggerHelper.ExecutedFunctionMessage(ex, fromEmail, deletedBookings, newBooking, clashedBookings));
                    _logger.ErrorException("Unable to send personal email for booking deletions by admin. Owner: " + owner, ex);
                }

            }
        }

        private void SendEmailToDSSForCreate(string fromEmail, Booking newBooking, Location bookingsLocation)
        {
            try
            {
                string toEmail = bookingsLocation.LocationCredentials.Where(x => x.Department == LocationCredentials.DepartmentNames.dss.ToString()).Select(x => x.Email).FirstOrDefault();
                string body = _emailHelper.GetEmailMarkup("~/Views/EmailTemplates/DSSNewBooking.cshtml", newBooking);

                if (!string.IsNullOrEmpty(body) && !string.IsNullOrEmpty(toEmail))
                {
                    _emailHelper.SendEmail(fromEmail, toEmail, string.Format("SMART room set up support on {0}", newBooking.StartDate.ToShortDateString()), body);
                    _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, fromEmail, newBooking, bookingsLocation));
                }
                else
                    throw new Exception("Body or To-Email is null.");
            }
            catch (Exception ex)
            {
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(ex, fromEmail, newBooking, bookingsLocation));
                _logger.ErrorException("Unable to send E-Mail to DSS for new booking: " + newBooking.ID, ex);
            }
        }

        private void SendEmailToSecurityForCreate(string fromEmail, Booking newBooking, Location bookingsLocation)
        {
            string toEmail = bookingsLocation.LocationCredentials.Where(x => x.Department == LocationCredentials.DepartmentNames.security.ToString()).Select(x => x.Email).FirstOrDefault();
            if (toEmail != null)
            {
                try
                {
                    string body = _emailHelper.GetEmailMarkup("~/Views/EmailTemplates/SecurityNewBooking.cshtml", newBooking);

                    if (!string.IsNullOrEmpty(body))
                    {
                        _emailHelper.SendEmail(fromEmail, toEmail, string.Format("External guests notifcation for {0}", newBooking.StartDate.ToShortDateString()), body);
                        _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, fromEmail, newBooking, bookingsLocation));
                    }
                    else
                        throw new Exception("Body is null.");
                }
                catch (Exception ex)
                {
                    _logger.Trace(LoggerHelper.ExecutedFunctionMessage(ex, fromEmail, newBooking, bookingsLocation));
                    _logger.ErrorException("Unable to send E-Mail to security for new booking: " + newBooking.ID, ex);
                }
            }
            else
            {
                try
                {
                    toEmail = _directoryService.GetUser(new User.Pid(newBooking.PID)).EmailAddress;
                    string body = _emailHelper.GetEmailMarkup("~/Views/EmailTemplates/SecurityNewBookingBookersCopy.cshtml", newBooking);

                    if (!string.IsNullOrEmpty(body) && !string.IsNullOrEmpty(toEmail))
                    {
                        _emailHelper.SendEmail(fromEmail, toEmail, string.Format("External guests security information for {0}", newBooking.StartDate.ToShortDateString()), body);
                        _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, fromEmail, newBooking, bookingsLocation));
                    }
                    else
                        throw new Exception("Body or To-Email is null.");
                }
                catch (Exception ex)
                {
                    _logger.Trace(LoggerHelper.ExecutedFunctionMessage(ex, fromEmail, newBooking, bookingsLocation));
                    _logger.ErrorException("Unable to send E-Mail to security for new booking: " + newBooking.ID, ex);
                }
            }
        }

        private void SendEmailToFacilitiesForCreate(Booking newBooking, string fromEmail, Location bookingsLocation)
        {
            try
            {
                string toEmail = bookingsLocation.LocationCredentials.Where(x => x.Department == LocationCredentials.DepartmentNames.facilities.ToString()).Select(x => x.Email).FirstOrDefault();
                string body = _emailHelper.GetEmailMarkup("~/Views/EmailTemplates/FacilitiesNewBooking.cshtml", newBooking);

                if (!string.IsNullOrEmpty(body) && !string.IsNullOrEmpty(toEmail))
                {
                    _emailHelper.SendEmail(fromEmail, toEmail, string.Format("Meeting room equipment booking on {0}", newBooking.StartDate.ToShortDateString()), body);
                    _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, fromEmail, bookingsLocation));
                }
                else
                    throw new Exception("Body or To-Email is null.");
            }
            catch (Exception ex)
            {
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(ex, fromEmail, bookingsLocation));
                _logger.ErrorException("Unable to send E-Mail to facilities for new booking: " + newBooking.ID, ex);
            }
        }

        private void SendEmailToOwnerForAdminCreate(string fromEmail, User user, Booking newBooking, List<Booking> bookingsToCreate)
        {
            try
            {
                string toEmail = _directoryService.GetUser(new User.Pid(newBooking.PID)).EmailAddress;

                string body = null;

                if (newBooking.IsSmartMeeting)
                    body = _emailHelper.GetEmailMarkup("~/Views/EmailTemplates/AdminNewSmartBooking.cshtml", bookingsToCreate);
                else
                {
                    if (newBooking.Recurrence.IsRecurring)
                        body = _emailHelper.GetEmailMarkup("~/Views/EmailTemplates/AdminNewRecurringBooking.cshtml", new Models.ViewModels.NewRecurringBookingWithMessage(newBooking, GetRecurrenceSentance(newBooking)));
                    else
                        body = _emailHelper.GetEmailMarkup("~/Views/EmailTemplates/AdminNewBooking.cshtml", newBooking);
                }

                if (!string.IsNullOrEmpty(body) && !string.IsNullOrEmpty(toEmail))
                {
                    _emailHelper.SendEmail(fromEmail, toEmail, "Meeting room ooking confirmation", body);
                    _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, fromEmail, user, newBooking, bookingsToCreate));
                }
                else
                    throw new Exception("Body or To-Email is null.");
            }
            catch (Exception ex)
            {
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(ex, fromEmail, user, newBooking, bookingsToCreate));
                _logger.ErrorException("Unable to send personal email for new booking: " + newBooking.ID, ex);
            }
        }

        private void SendEmailToOwnerForCreate(string fromEmail, User user, Booking newBooking, List<Booking> bookingsToCreate)
        {
            try
            {
                string toEmail = _directoryService.GetUser(new User.Pid(newBooking.PID)).EmailAddress;

                string body = null;

                if (newBooking.IsSmartMeeting)
                    bookingsToCreate = GetRoomForBookings(bookingsToCreate);
                else
                    newBooking.Room = _roomsRepository.GetRoomById(newBooking.RoomID);

                if (newBooking.IsSmartMeeting)
                    body = _emailHelper.GetEmailMarkup("~/Views/EmailTemplates/NewSmartBooking.cshtml", bookingsToCreate);
                else
                {
                    if (newBooking.Recurrence.IsRecurring)
                        body = _emailHelper.GetEmailMarkup("~/Views/EmailTemplates/NewRecurringBooking.cshtml", new Models.ViewModels.NewRecurringBookingWithMessage(newBooking, GetRecurrenceSentance(newBooking)));
                    else
                        body = _emailHelper.GetEmailMarkup("~/Views/EmailTemplates/NewBooking.cshtml", newBooking);
                }

                if (!string.IsNullOrEmpty(body) && !string.IsNullOrEmpty(toEmail))
                {
                    _emailHelper.SendEmail(fromEmail, toEmail, "Meeting room booking confirmation", body);
                    _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, fromEmail, user, newBooking, bookingsToCreate));
                }
                else
                    throw new Exception("Body or To-Email is null.");
            }
            catch (Exception ex)
            {
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(ex, fromEmail, fromEmail, user, newBooking, bookingsToCreate));
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

        public class UnableToEditBookingException : Exception { }

        public class DeletionException : Exception { }
    }
}