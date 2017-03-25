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
using System.Diagnostics;
using System.IO;
using System.Data.Entity;
using System.Configuration;
using System.Web.Script.Serialization;

using System.Linq.Expressions;
using System.Linq.Dynamic;
using VORBS.Services;
using VORBS.DAL.Repositories;
using VORBS.Models.ViewModels;

namespace VORBS.API
{
    [RoutePrefix("api/bookings")]
    public class BookingsController : ApiController
    {
        private NLog.Logger _logger;
        private VORBSContext db;
        private BookingRepository _bookingsRepository;
        private LocationRepository _locationsRepository;
        private RoomRepository _roomsRepository;

        private IDirectoryService _directoryService;

        public BookingsController() : this(new VORBSContext()) { }

        public BookingsController(VORBSContext context)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
            db = context;
            _bookingsRepository = new BookingRepository(db);
            _locationsRepository = new LocationRepository(db);
            _roomsRepository = new RoomRepository(db);
            _directoryService = new StubbedDirectoryService();
        }

        [Route("{location}/{start:DateTime}/{end:DateTime}/")]
        [HttpGet]
        public List<BookingDTO> GetRoomBookingsForLocation(string location, DateTime start, DateTime end)
        {
            if (location == null)
                return new List<BookingDTO>();

            List<BookingDTO> bookingsDTO = new List<BookingDTO>();

            try
            {
                Location currentLocation = _locationsRepository.GetLocationByName(location);
                List<Booking> bookings = _bookingsRepository.GetByDateAndLocation(start, end, currentLocation);

                bookings.ForEach(x => bookingsDTO.Add(new BookingDTO()
                {
                    ID = x.ID,
                    EndDate = x.EndDate,
                    StartDate = x.StartDate,
                    Owner = x.Owner,
                    IsSmartMeeting = x.IsSmartMeeting,
                    RecurrenceId = x.RecurrenceId,
                    ExternalAttendees = x.ExternalAttendees.Select(y =>
                    {
                        return new ExternalAttendeesDTO()
                        {
                            ID = y.ID,
                            BookingID = y.BookingID,
                            FullName = y.FullName,
                            CompanyName = y.CompanyName,
                            PassRequired = y.PassRequired
                        };
                    }),
                    Location = new LocationDTO()
                    {
                        ID = x.Room.Location.ID,
                        Name = x.Room.Location.Name,
                        LocationCredentials = x.Room.Location.LocationCredentials.ToList().Select(l => { return new LocationCredentialsDTO() { Department = l.Department, Email = l.Email, ID = l.ID, LocationID = l.LocationID, PhoneNumber = l.PhoneNumber }; }).ToList()
                    },
                    Room = new RoomDTO() { ID = x.Room.ID, RoomName = x.Room.RoomName, ComputerCount = x.Room.ComputerCount, PhoneCount = x.Room.PhoneCount, SmartRoom = x.Room.SmartRoom }
                }));
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get bookings for location: " + location, ex);
            }
            return bookingsDTO;
        }

        [Route("{location}/{room}/{start:DateTime}/{end:DateTime}")]
        [HttpGet]
        public List<BookingDTO> GetRoomBookingsForRoom(string location, DateTime start, DateTime end, string room)
        {
            if (location == null || room == null)
                return new List<BookingDTO>();

            List<BookingDTO> bookingsDTO = new List<BookingDTO>();

            try
            {

                Room currentRoom = null;
                if (room != null)
                    currentRoom = _roomsRepository.GetRoomByName(room);

                List<Booking> bookings = _bookingsRepository.GetByDateAndRoom(start, end, currentRoom);


                bookings.ForEach(x => bookingsDTO.Add(new BookingDTO()
                {
                    ID = x.ID,
                    EndDate = x.EndDate,
                    StartDate = x.StartDate,
                    Owner = x.Owner,
                    IsSmartMeeting = x.IsSmartMeeting,
                    RecurrenceId = x.RecurrenceId,
                    ExternalAttendees = x.ExternalAttendees.Select(y =>
                    {
                        return new ExternalAttendeesDTO()
                        {
                            ID = y.ID,
                            BookingID = y.BookingID,
                            FullName = y.FullName,
                            CompanyName = y.CompanyName,
                            PassRequired = y.PassRequired
                        };
                    }),
                    Location = new LocationDTO()
                    {
                        ID = x.Room.Location.ID,
                        Name = x.Room.Location.Name,
                        LocationCredentials = x.Room.Location.LocationCredentials.ToList().Select(l => { return new LocationCredentialsDTO() { Department = l.Department, Email = l.Email, ID = l.ID, LocationID = l.LocationID, PhoneNumber = l.PhoneNumber }; }).ToList()
                    },
                    Room = new RoomDTO() { ID = x.Room.ID, RoomName = x.Room.RoomName, ComputerCount = x.Room.ComputerCount, PhoneCount = x.Room.PhoneCount, SmartRoom = x.Room.SmartRoom }
                }));
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get bookings for room: " + location + "/" + room, ex);
            }
            return bookingsDTO;
        }

        [Route("{location}/{room}/{start:DateTime}/{end:DateTime}/{person}")]
        [HttpGet]
        public List<BookingDTO> GetRoomBookingsForRoomAndPerson(string location, DateTime start, DateTime end, string room, string person)
        {
            if (location == null || room == null || person == null)
                return new List<BookingDTO>();

            List<BookingDTO> bookingsDTO = new List<BookingDTO>();

            try
            {
                Room currentRoom = null;
                if (room != null)
                    currentRoom = _roomsRepository.GetRoomByName(room);

                List<Booking> bookings = _bookingsRepository.GetByDateRoomAndOwner(start, end, currentRoom, person);

                bookings.ForEach(x => bookingsDTO.Add(new BookingDTO()
                {
                    ID = x.ID,
                    EndDate = x.EndDate,
                    StartDate = x.StartDate,
                    Owner = x.Owner,
                    IsSmartMeeting = x.IsSmartMeeting,
                    RecurrenceId = x.RecurrenceId,
                    ExternalAttendees = x.ExternalAttendees.Select(y =>
                    {
                        return new ExternalAttendeesDTO()
                        {
                            ID = y.ID,
                            BookingID = y.BookingID,
                            FullName = y.FullName,
                            CompanyName = y.CompanyName,
                            PassRequired = y.PassRequired
                        };
                    }),
                    Location = new LocationDTO()
                    {
                        ID = x.Room.Location.ID,
                        Name = x.Room.Location.Name,
                        LocationCredentials = x.Room.Location.LocationCredentials.ToList().Select(l => { return new LocationCredentialsDTO() { Department = l.Department, Email = l.Email, ID = l.ID, LocationID = l.LocationID, PhoneNumber = l.PhoneNumber }; }).ToList()
                    },
                    Room = new RoomDTO() { ID = x.Room.ID, RoomName = x.Room.RoomName, ComputerCount = x.Room.ComputerCount, PhoneCount = x.Room.PhoneCount, SmartRoom = x.Room.SmartRoom }
                }));
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get bookings for room and person: " + location + "/" + room + "/" + person, ex);
            }
            return bookingsDTO;
        }

        [Route("{start:DateTime}/{person}")]
        [HttpGet]
        public List<BookingDTO> GetAllRoomBookingsForCurrentUser(DateTime start, string person)
        {
            List<BookingDTO> bookingsDTO = new List<BookingDTO>();

            try
            {
                string currentPid = User.Identity.Name.Substring(User.Identity.Name.IndexOf("\\") + 1);

                List<Booking> bookings = _bookingsRepository.GetByDateAndPid(start, currentPid);

                bookings.ForEach(x => bookingsDTO.Add(new BookingDTO()
                {
                    ID = x.ID,
                    EndDate = x.EndDate,
                    StartDate = x.StartDate,
                    Subject = x.Subject,
                    Owner = x.Owner,
                    IsSmartMeeting = x.IsSmartMeeting,
                    RecurrenceId = x.RecurrenceId,
                    ExternalAttendees = x.ExternalAttendees.Select(y =>
                    {
                        return new ExternalAttendeesDTO()
                        {
                            ID = y.ID,
                            BookingID = y.BookingID,
                            FullName = y.FullName,
                            CompanyName = y.CompanyName,
                            PassRequired = y.PassRequired
                        };
                    }),
                    Location = new LocationDTO()
                    {
                        ID = x.Room.Location.ID,
                        Name = x.Room.Location.Name,
                        LocationCredentials = x.Room.Location.LocationCredentials.ToList().Select(l => { return new LocationCredentialsDTO() { Department = l.Department, Email = l.Email, ID = l.ID, LocationID = l.LocationID, PhoneNumber = l.PhoneNumber }; }).ToList()
                    },
                    Room = new RoomDTO()
                    {
                        ID = x.Room.ID,
                        RoomName = x.Room.RoomName,
                        ComputerCount = x.Room.ComputerCount,
                        PhoneCount = x.Room.PhoneCount,
                        SmartRoom = x.Room.SmartRoom,
                        SeatCount = x.Room.SeatCount
                    }
                }));
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get bookings for current user", ex);
            }
            return bookingsDTO;
        }

        [Route("{bookingId:int}")]
        [HttpGet]
        public BookingDTO GetRoomBookingsForBookingId(int bookingId)
        {
            BookingDTO bookingsDTO = new BookingDTO();

            try
            {
                Booking booking = _bookingsRepository.GetById(bookingId);

                bookingsDTO = new BookingDTO()
                {
                    ID = booking.ID,
                    EndDate = booking.EndDate,
                    StartDate = booking.StartDate,
                    Subject = booking.Subject,
                    Owner = booking.Owner,
                    NumberOfAttendees = booking.NumberOfAttendees,
                    RecurrenceId = booking.RecurrenceId,
                    ExternalAttendees = booking.ExternalAttendees.Select(x =>
                    {
                        return new ExternalAttendeesDTO()
                        {
                            ID = x.ID,
                            BookingID = x.BookingID,
                            FullName = x.FullName,
                            CompanyName = x.CompanyName,
                            PassRequired = x.PassRequired
                        };
                    }),
                    Flipchart = booking.Flipchart,
                    Projector = booking.Projector,
                    DssAssist = booking.DssAssist,
                    PID = booking.PID,
                    IsSmartMeeting = booking.IsSmartMeeting,
                    Location = new LocationDTO()
                    {
                        ID = booking.Room.Location.ID,
                        Name = booking.Room.Location.Name,
                        LocationCredentials = booking.Room.Location.LocationCredentials.ToList().Select(l => { return new LocationCredentialsDTO() { Department = l.Department, Email = l.Email, ID = l.ID, LocationID = l.LocationID, PhoneNumber = l.PhoneNumber }; }).ToList()
                    },
                    Room = new RoomDTO()
                    {
                        ID = booking.Room.ID,
                        RoomName = booking.Room.RoomName,
                        ComputerCount = booking.Room.ComputerCount,
                        PhoneCount = booking.Room.PhoneCount,
                        SmartRoom = booking.Room.SmartRoom,
                        SeatCount = booking.Room.SeatCount
                    }
                };

                return bookingsDTO;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get booking by id :" + bookingId, ex);
            }
            return bookingsDTO;
        }

        [HttpPost]
        public HttpResponseMessage SaveNewBooking(Booking newBooking)
        {
            //PLEASE SORT THIS MONSTROSITY OUT.. ITS... SO... *cries*
            try
            {
                List<Booking> bookingsToCreate = new List<Booking>();
                List<Booking> clashedBookings = new List<Booking>();
                List<Booking> deletedBookings = new List<Booking>();

                List<DateTime> recurringDates = new List<DateTime>();

                Room bookingRoom = _roomsRepository.GetRoomById(newBooking.RoomID);
                newBooking.RoomID = newBooking.Room.ID;

                int? nextRecurringId = null;

                bool doMeetingsClash = false;

                //Get the current user
                if (string.IsNullOrWhiteSpace(newBooking.PID) || string.IsNullOrWhiteSpace(newBooking.Owner))
                {
                    User user = _directoryService.GetCurrentUser(User.Identity.Name);

                    if (user == null)
                        return Request.CreateResponse(HttpStatusCode.NotFound, "User not found in Active Directory. " + User.Identity.Name);

                    newBooking.Owner = user.FullName;
                    newBooking.PID = user.PayId.Identity;
                }

                if (newBooking.Recurrence.IsRecurring)
                {
                    AvailabilityController aC = new AvailabilityController();

                    recurringDates = GetDatesForRecurrencePeriod(newBooking.StartDate, newBooking.Recurrence);
                    nextRecurringId = _bookingsRepository.GetNextRecurrenceId();

                    if (newBooking.SmartLoactions.Count() > 0 && newBooking.Room.SmartRoom)
                    {
                        var smartBookings = _bookingsRepository.GetAvailableSmartRoomBookings(newBooking, out clashedBookings);

                        //No Rooms avalible; Show clashes to users
                        if (clashedBookings.Count() > 0)
                        {
                            var clashedBookingsString = new JavaScriptSerializer().Serialize(ConvertBookingsToDTOs(clashedBookings));
                            return Request.CreateErrorResponse(HttpStatusCode.BadGateway, clashedBookingsString);
                        }

                        newBooking.IsSmartMeeting = true;
                        smartBookings.Add(newBooking);

                        foreach (var smartBooking in smartBookings)
                            bookingsToCreate.AddRange(GetBookingsForRecurringDates(recurringDates, smartBooking));
                    }
                    else
                        bookingsToCreate.AddRange(GetBookingsForRecurringDates(recurringDates, newBooking));

                    doMeetingsClash = aC.DoMeetingsClashRecurringly(bookingsToCreate.Select(x => x.Room).OrderBy(y => y.Location.ID).ToList(), TimeSpan.Parse(newBooking.StartDate.ToShortTimeString()), TimeSpan.Parse(newBooking.EndDate.ToShortTimeString()), recurringDates, out clashedBookings);

                    if (doMeetingsClash)
                    {
                        if (newBooking.Recurrence.SkipClashes)
                        {
                            bookingsToCreate.RemoveAll(x => clashedBookings.Select(c => c.StartDate.ToShortDateString()).Contains(x.StartDate.ToShortDateString()));
                        }
                        else if (newBooking.Recurrence.AutoAlternateRoom)
                        {
                            foreach (var cB in clashedBookings)
                            {
                                Room newRoom;
                                if (newBooking.SmartLoactions.Count() > 0 && newBooking.Room.SmartRoom) //TODO: Change when we introduce new validation check in UI
                                {
                                    var unAvaliableRooms = bookingsToCreate.Where(y => cB.Room.LocationID == y.Room.LocationID && y.RoomID != cB.RoomID).Select(x => x.RoomID).Distinct();
                                    newRoom = aC.GetAlternateSmartRoom(unAvaliableRooms, cB.StartDate, cB.EndDate, cB.Room.LocationID);
                                }
                                else
                                {
                                    TimeSpan startTime = new TimeSpan(newBooking.StartDate.Hour, newBooking.StartDate.Minute, newBooking.StartDate.Second);
                                    TimeSpan endTime = new TimeSpan(newBooking.EndDate.Hour, newBooking.EndDate.Minute, newBooking.EndDate.Second);

                                    newRoom = aC.GetAlternateRoom(startTime, endTime, newBooking.Room.SeatCount, cB.Room.LocationID, true);
                                }

                                if (newRoom == null)
                                {
                                    var clashedBookingsString = new JavaScriptSerializer().Serialize(new[] { ConvertBookingToDTO(cB) });
                                    return Request.CreateErrorResponse(HttpStatusCode.BadGateway, clashedBookingsString);
                                }

                                Booking newClashedBooking = bookingsToCreate.First(x => x.RoomID == cB.RoomID && cB.StartDate == x.StartDate && cB.EndDate == x.EndDate);

                                newClashedBooking.Room = newRoom;
                                newClashedBooking.RoomID = newRoom.ID;
                            }
                        }
                        else if (newBooking.Recurrence.AdminOverwrite)
                        {
                            //checking they are still admin
                            if (VORBS.Security.VorbsAuthorise.IsUserAuthorised(User.Identity.Name, 1))
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
                            {
                                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Unauthorised to overwrite bookings.");
                            }
                        }
                        else
                        {
                            var clashedBookingsString = new JavaScriptSerializer().Serialize(ConvertBookingsToDTOs(clashedBookings));
                            return Request.CreateErrorResponse(HttpStatusCode.Conflict, clashedBookingsString);
                        }
                    }
                }
                else if (newBooking.SmartLoactions.Count() > 0 && newBooking.Room.SmartRoom) //TODO: Change when we introduce new validation check in UI
                {
                    var smartBookings = _bookingsRepository.GetAvailableSmartRoomBookings(newBooking, out clashedBookings);

                    //No Rooms avalible; Show clashes to users
                    if (clashedBookings.Count() > 0)
                    {
                        var clashedBookingsString = new JavaScriptSerializer().Serialize(ConvertBookingsToDTOs(clashedBookings));
                        return Request.CreateErrorResponse(HttpStatusCode.BadGateway, clashedBookingsString);
                    }

                    newBooking.IsSmartMeeting = true;

                    bookingsToCreate.Add(newBooking);
                    bookingsToCreate.AddRange(smartBookings);
                }
                else
                    bookingsToCreate.Add(newBooking);

                //Reset  Room as we dont want to create another room
                bookingsToCreate.ForEach(x => x.Room = null);

                //add the recurrence link id if needed
                if (nextRecurringId != null && nextRecurringId > 0)
                    bookingsToCreate.ForEach(x => x.RecurrenceId = nextRecurringId);

                _bookingsRepository.SaveNewBookings(bookingsToCreate);

                if (deletedBookings.Count > 0)
                {
                    foreach (string owner in deletedBookings.Select(x => x.PID).Distinct())
                    {
                        try
                        {
                            //Once Booking has been removed; Send Cancelltion Emails
                            string toEmail = _directoryService.GetUser(new User.Pid(owner)).EmailAddress;
                            string errorMessage = (newBooking.Recurrence.AdminOverwriteMessage.Trim().Length > 0) ? newBooking.Recurrence.AdminOverwriteMessage : "An admin has cancelled these bookings.";
                            string body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/AdminMultiCancelledBookingWithMessage.cshtml", new Models.ViewModels.AdminMultiCancelledBookingWithMessage(clashedBookings.Where(x => x.PID == owner), errorMessage));

                            Utils.EmailHelper.SendEmail(ConfigurationManager.AppSettings["fromEmail"], toEmail, "Meeting room booking(s) cancellation", body);
                        }
                        catch (Exception exn)
                        {
                            _logger.ErrorException("Unable to send personal email for booking deletions by admin. Owner: " + owner, exn);
                        }

                    }
                }

                newBooking.Room = bookingRoom;

                _logger.Info("Booking successfully created: " + newBooking.ID);

                if (bool.Parse(ConfigurationManager.AppSettings["sendEmails"]))
                    return new HttpResponseMessage(HttpStatusCode.OK);

                string fromEmail = ConfigurationManager.AppSettings["fromEmail"];
                newBooking.Room = _roomsRepository.GetRoomById(newBooking.RoomID);
                try
                {
                    string currentUserPid = User.Identity.Name.Substring(User.Identity.Name.IndexOf("\\") + 1);
                    string toEmail = _directoryService.GetUser(new User.Pid(newBooking.PID)).EmailAddress;

                    string body = "";

                    if (newBooking.IsSmartMeeting)
                        bookingsToCreate = GetRoomForBookings(bookingsToCreate);
                    else
                        newBooking.Room = _roomsRepository.GetRoomById(newBooking.RoomID);


                    if (newBooking.PID.ToUpper() != currentUserPid.ToUpper())
                        if (newBooking.IsSmartMeeting)
                            body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/AdminNewSmartBooking.cshtml", bookingsToCreate);
                        else
                        {
                            if (newBooking.Recurrence.IsRecurring)
                                body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/AdminNewRecurringBooking.cshtml", new Models.ViewModels.NewRecurringBookingWithMessage(newBooking, GetRecurrenceSentance(newBooking)));
                            else
                                body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/AdminNewBooking.cshtml", newBooking);
                        }
                    else
                    {
                        if (newBooking.IsSmartMeeting)
                            body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/NewSmartBooking.cshtml", bookingsToCreate);
                        else
                        {
                            if (newBooking.Recurrence.IsRecurring)
                                body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/NewRecurringBooking.cshtml", new Models.ViewModels.NewRecurringBookingWithMessage(newBooking, GetRecurrenceSentance(newBooking)));
                            else
                                body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/NewBooking.cshtml", newBooking);
                        }
                    }

                    Utils.EmailHelper.SendEmail(fromEmail, toEmail, "Meeting room booking confirmation", body);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Unable to send personal email for new booking: " + newBooking.ID, ex);
                }


                //need location to get DSO, security specific emails etc..
                Location bookingsLocation = _roomsRepository.GetRoomById(newBooking.RoomID).Location;
                if (newBooking.Flipchart || newBooking.Projector)
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

                if (newBooking.ExternalAttendees != null && newBooking.ExternalAttendees.Count > 0)
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

                if (newBooking.DssAssist)
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

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (BookingConflictException ex)
            {
                _logger.FatalException("Unable to save new booking: " + newBooking.Owner + "/" + newBooking.StartDate, ex);
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.FatalException("Unable to save new booking: " + newBooking.Owner + "/" + newBooking.StartDate, ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [Route("{existingBookingId:int}/{recurrence:bool?}")]
        public HttpResponseMessage EditExistingBooking(int existingBookingId, Booking editBooking, bool? recurrence = false)
        {

            //Find Existing Booking
            Booking existingBooking = _bookingsRepository.GetById(existingBookingId);

            int? recurringBookingId = null;
            if (recurrence.Value)
                recurringBookingId = existingBooking.RecurrenceId;
            List<Booking> allBookings = (recurringBookingId == null) ? new List<Booking> { existingBooking } : _bookingsRepository.GetBookingsInRecurrence(recurringBookingId.Value);
            List<Booking> originalBookings = (recurringBookingId == null) ? new List<Booking> { existingBooking } : _bookingsRepository.GetBookingsInRecurrence(recurringBookingId.Value, true);

            List<Booking> editBookings = new List<Booking>();

            try
            {
                
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
                }


                var updatedBookings = _bookingsRepository.UpdateExistingBookings(allBookings, editBookings);
                if (updatedBookings == null)
                {
                    _logger.Fatal("Unable to edit booking(s): " + editBooking.ID);
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "An error occured whilst updating the booking(s). Please try again or contact Service Desk");
                }
                else
                {
                    _logger.Info("Booking(s) successfully editted: " + String.Join(", ", editBookings.Select(x => x.ID)));
                }

                string body = "";
                string dssBody = "";
                string ownerBody = "";

                string facilitiesEmail = "";
                string dssEmail = "";

                string fromEmail = ConfigurationManager.AppSettings["fromEmail"];

                _logger.Info("Booking(s) successfully editted: " + String.Join(",", editBookings.Select(x => x.ID)));

                //Get markup for facilities email
                Location bookingsLocation = _roomsRepository.GetRoomById(editBooking.RoomID).Location;
                List<FacilitiesEdittedBooking> facilitiesViewModels = new List<FacilitiesEdittedBooking>();
                foreach (Booking booking in editBookings)
                {
                    Booking currentBooking = originalBookings.Where(x => x.ID == booking.ID).First();
                    if ((currentBooking.Flipchart != booking.Flipchart) || (booking.Projector != currentBooking.Projector))
                    {
                        facilitiesEmail = bookingsLocation.LocationCredentials.Where(x => x.Department == LocationCredentials.DepartmentNames.facilities.ToString()).Select(x => x.Email).FirstOrDefault();
                        if (facilitiesEmail != null)
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
                }
                try
                {
                    if (facilitiesViewModels.Count > 0)
                        body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/FacilitiesEdittedBooking.cshtml", facilitiesViewModels);
                }
                catch (Exception exn)
                {
                    _logger.ErrorException("Unable to retrieve email markup for facilities for editted booking(s): " + String.Join(", ", editBookings.Select(x => x.ID)), exn);
                }


                //get markup for dss email
                List<Booking> dssEditBookings = new List<Booking>();
                foreach (Booking booking in editBookings)
                {
                    Booking currentBooking = originalBookings.Where(x => x.ID == booking.ID).First();
                    if (booking.DssAssist && !currentBooking.DssAssist)
                    {
                        dssEmail = bookingsLocation.LocationCredentials.Where(x => x.Department == LocationCredentials.DepartmentNames.dss.ToString()).Select(x => x.Email).FirstOrDefault();
                        if (dssEmail != null)
                        {
                            booking.Room.Location = currentBooking.Room.Location;
                            dssEditBookings.Add(booking);
                        }
                    }
                }
                try
                {
                    if(dssEditBookings.Count > 0)
                        dssBody = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/DSSEdittedBooking.cshtml", dssEditBookings);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Unable to retrieve E-Mail markup for DSS for new booking(s): " + String.Join(", ", editBookings.Select(x => x.ID)), ex);
                }



                string securityEmailBody = "";
                string securityEmail = "";

                //get markup for security email
                List<SecurityEdittedBooking> securityViewModels = new List<SecurityEdittedBooking>();
                foreach (Booking booking in editBookings)
                {
                    Booking currentBooking = originalBookings.Where(x => x.ID == booking.ID).First();

                    if (!(new HashSet<string>(currentBooking.ExternalAttendees.Select(x => x.FullName)).SetEquals(booking.ExternalAttendees.Select(x => x.FullName))))
                    {
                        securityEmail = bookingsLocation.LocationCredentials.Where(x => x.Department == LocationCredentials.DepartmentNames.security.ToString()).Select(x => x.Email).FirstOrDefault();
                        IEnumerable<ExternalAttendees> removedAttendees = currentBooking.ExternalAttendees.Where(x => !booking.ExternalAttendees.Select(y => y.FullName).Contains(x.FullName));
                        IEnumerable<ExternalAttendees> addedAttendees = booking.ExternalAttendees.Where(x => !currentBooking.ExternalAttendees.Select(y => y.FullName).Contains(x.FullName));
                        SecurityEdittedBooking viewModel = new Models.ViewModels.SecurityEdittedBooking() { EdittedBooking = booking, NewAttendees = addedAttendees, RemovedAttendees = removedAttendees };
                        securityViewModels.Add(viewModel);
                    }
                }
                try
                {
                    if(securityViewModels.Count > 0)
                        securityEmailBody = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/SecurityEdittedBooking.cshtml", securityViewModels);
                }
                catch (Exception exn)
                {
                    _logger.ErrorException("Unable to retrieve security email markup for editting booking(s): " + String.Join(", ", editBookings.Select(x => x.ID)), exn);
                }

                //get markup for owner email
                string currentUserPid = User.Identity.Name.Substring(User.Identity.Name.IndexOf("\\") + 1);
                string toOwnerEmail = _directoryService.GetUser(new User.Pid(existingBooking.PID)).EmailAddress;
                string toOwnerSubject = "";

                if (existingBooking.PID.ToUpper() != currentUserPid.ToUpper())
                {
                    ownerBody = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/AdminEdittedBooking.cshtml", editBookings);
                    toOwnerSubject = "Meeting room edit confirmation";
                }
                else
                {
                    ownerBody = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/EdittedBooking.cshtml", editBookings);
                    toOwnerSubject = "Meeting room booking confirmation";
                }

                //END OF EMAIL BODY CONSTRUCTIONS

                if (Boolean.Parse(ConfigurationManager.AppSettings["sendEmails"]))
                    return new HttpResponseMessage(HttpStatusCode.OK);
                
                //Send facilities Email
                try
                {
                    if (!string.IsNullOrEmpty(body))
                        Utils.EmailHelper.SendEmail(fromEmail, facilitiesEmail, "(Updated) Meeting room equipment for booking(s)", body);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Unable to send E-Mail to facilities for editting booking(s): " + String.Join(", ", editBookings.Select(x => x.ID)), ex);
                }

                //Send DSS Email
                try
                {
                    if (!string.IsNullOrEmpty(dssBody))
                        Utils.EmailHelper.SendEmail(fromEmail, dssEmail, "(Updated) SMART room set up support", dssBody);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Unable to send E-Mail to DSS for editting booking(s): " + String.Join(", ", editBookings.Select(x => x.ID)), ex);
                }

                //Send security Email
                try
                {
                    if (!string.IsNullOrEmpty(securityEmailBody) && !string.IsNullOrEmpty(securityEmail))
                        Utils.EmailHelper.SendEmail(fromEmail, securityEmail, "(Updated) External guests notification", securityEmailBody);
                }
                catch (Exception exn)
                {
                    _logger.ErrorException("Unable to send E-Mail to security for editting booking(s): " + String.Join(", ", editBookings.Select(x => x.ID)), exn);
                }
                
                //Send Owner Email
                try
                {
                    if (!string.IsNullOrEmpty(ownerBody) && !string.IsNullOrEmpty(toOwnerEmail))
                    {
                        Utils.EmailHelper.SendEmail(fromEmail, toOwnerEmail, toOwnerSubject, ownerBody);
                    }
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Unable to send personal email for editting booking(s): " + String.Join(", ", editBookings.Select(x => x.ID)), ex);
                }
                
                
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (BookingConflictException ex)
            {
                _logger.FatalException("Unable to edit booking(s): " + editBooking.Owner + "/ ids:" + String.Join(", ", editBookings.Select(x => x.ID)), ex);
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.FatalException("Unable to edit booking(s): " + String.Join(", ", editBookings.Select(x => x.ID)), ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("{bookingId:Int}/{recurrence:bool?}")]
        [HttpDelete]
        public HttpResponseMessage DeleteBookingById(int bookingId, bool? recurrence = false)
        {
            try
            {
                Booking booking = _bookingsRepository.GetById(bookingId);
                Location bookingsLocation =_roomsRepository.GetRoomById(booking.RoomID).Location;

                string fromEmail = ConfigurationManager.AppSettings["fromEmail"];

                //NOT NEEDED RIGHT NOW, BUT WILL SEND SECURITY E-MAILS WHEN NEEDED.
                //string securityEmailBody = "";
                //if (booking.ExternalAttendees.Count > 0)
                //{
                //    securityEmailBody = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/SecurityDeletedBooking.cshtml", booking);
                //}


                //    if (!_bookingService.DeleteById(bookingId))
                //    {
                //        return Request.CreateResponse(HttpStatusCode.InternalServerError, "Unable to update existing booking. An error occured, please contact help desk.");
                //    }
                //    _logger.Info("Booking successfully cancelled: " + bookingId);


                int? recurringBookingId = null;
                if (recurrence.Value)
                    recurringBookingId = booking.RecurrenceId;
                List<Booking> allBookings = (recurringBookingId == null) ? new List<Booking> { booking } : _bookingsRepository.GetBookingsInRecurrence(recurringBookingId.Value);

                bool success = _bookingsRepository.Delete(allBookings);
                if (!success)
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Unable to update existing booking. An error occured, please contact help desk.");
                else
                    _logger.Info("Booking(s) successfully cancelled: " + String.Join(", ", allBookings.Select(x => x.ID)));

                if (bool.Parse(ConfigurationManager.AppSettings["sendEmails"]))
                    return new HttpResponseMessage(HttpStatusCode.OK);

                Room locationRoom = _roomsRepository.GetRoomById(booking.RoomID);
                allBookings.ForEach(x => x.Room = locationRoom);

                string body = "";
                try
                {
                    //Once Booking has been removed; Send Cancelltion Emails

                    string currentUserPid = User.Identity.Name.Substring(User.Identity.Name.IndexOf("\\") + 1);
                    
                    string toEmail = _directoryService.GetUser(new User.Pid(booking.PID)).EmailAddress;


                    if (booking.PID.ToUpper() != currentUserPid.ToUpper())
                        body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/AdminCancelledBooking.cshtml", allBookings);
                    else
                        body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/CancelledBooking.cshtml", allBookings);

                    Utils.EmailHelper.SendEmail(fromEmail, toEmail, "Meeting room booking cancellation(s)", body);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Unable to send personal email for deleting booking(s): " + String.Join(", ", allBookings.Select(x => x.ID)), ex);
                }


                if (booking.Flipchart || booking.Projector)
                {

                    string facilitiesEmail = bookingsLocation.LocationCredentials.Where(x => x.Department == LocationCredentials.DepartmentNames.facilities.ToString()).Select(x => x.Email).FirstOrDefault();
                    if (facilitiesEmail != null)
                    {
                        try
                        {
                            body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/FacilitiesDeletedBooking.cshtml", allBookings);
                            Utils.EmailHelper.SendEmail(fromEmail, facilitiesEmail, "Meeting room equipment cancellation for booking(s)", body);
                        }
                        catch (Exception ex)
                        {
                            _logger.ErrorException("Unable to send email to facilities for deleting booking(s): " + String.Join(", ", allBookings.Select(x => x.ID)), ex);
                        }
                    }
                }

                if (booking.DssAssist)
                {
                    string dssEmail = bookingsLocation.LocationCredentials.Where(x => x.Department == LocationCredentials.DepartmentNames.dss.ToString()).Select(x => x.Email).FirstOrDefault();
                    if (dssEmail != null)
                    {
                        try
                        {
                            body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/DSSDeletedBooking.cshtml", allBookings);
                            Utils.EmailHelper.SendEmail(fromEmail, dssEmail, "Meeting room booking cancellation(s)", body);
                        }
                        catch (Exception ex)
                        {
                            _logger.ErrorException("Unable to send email to dss for deleting booking(s): " + String.Join(", ", allBookings.Select(x => x.ID)), ex);
                        }
                    }
                }

                //NOT NEEDED RIGHT NOW, BUT WILL SEND SECURITY E-MAILS WHEN NEEDED.
                //string securityEmailAddress = bookingsLocation.LocationCredentials.Where(x => x.Department == LocationCredentials.DepartmentNames.security.ToString()).Select(x => x.Email).FirstOrDefault();
                //if (!string.IsNullOrEmpty(securityEmailBody) && !string.IsNullOrEmpty(securityEmailAddress))
                //{
                //    try
                //    {
                //        Utils.EmailHelper.SendEmail(fromEmail, securityEmailAddress, "Meeting room booking cancellation", securityEmailBody);
                //    }
                //    catch (Exception exn)
                //    {
                //        _logger.ErrorException("Unable to send email to security for deleting booking: " + bookingId, exn);
                //    }
                //}


                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.FatalException("Unable to delete booking: " + bookingId, ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("search")]
        [HttpGet]
        public List<BookingDTO> GetBookingByOwner(DateTime? start, string owner, string room, int? location = 0)
        {

            Room currentRoom = null;
            if (room != null)
                currentRoom = _roomsRepository.GetRoomByName(room);

            Location currentLocation = null;
            if (location != null && location != 0)
                currentLocation = _locationsRepository.GetLocationById(location.Value);

            var bookings = _bookingsRepository.GetByPartialDateRoomLocationSmartAndOwner(start, null, owner, null, currentRoom, currentLocation);

            List<BookingDTO> bookingsDTO = new List<BookingDTO>();
            bookings.ForEach(x => bookingsDTO.Add(new BookingDTO()
            {
                ID = x.ID,
                EndDate = x.EndDate,
                StartDate = x.StartDate,
                Subject = x.Subject,
                Owner = x.Owner,
                IsSmartMeeting = x.IsSmartMeeting,
                RecurrenceId = x.RecurrenceId,
                ExternalAttendees = x.ExternalAttendees.Select(y =>
                {
                    return new ExternalAttendeesDTO()
                    {
                        ID = y.ID,
                        BookingID = y.BookingID,
                        FullName = y.FullName,
                        CompanyName = y.CompanyName,
                        PassRequired = y.PassRequired
                    };
                }),
                Location = new LocationDTO()
                {
                    ID = x.Room.Location.ID,
                    Name = x.Room.Location.Name,
                    LocationCredentials = x.Room.Location.LocationCredentials.ToList().Select(l => { return new LocationCredentialsDTO() { Department = l.Department, Email = l.Email, ID = l.ID, LocationID = l.LocationID, PhoneNumber = l.PhoneNumber }; }).ToList()
                },
                Room = new RoomDTO() { ID = x.Room.ID, RoomName = x.Room.RoomName, ComputerCount = x.Room.ComputerCount, PhoneCount = x.Room.PhoneCount, SmartRoom = x.Room.SmartRoom }
            }));

            return bookingsDTO;
        }

        [HttpGet]
        [Route("{startDate:DateTime}/{period:int}")]
        public IEnumerable<BookingDTO> GetBookingsForPeriodAndCurrentUser(DateTime startDate, int period)
        {
            List<BookingDTO> bookingsDTO = new List<BookingDTO>();

            try
            {
                string currentPid = User.Identity.Name.Substring(User.Identity.Name.IndexOf("\\") + 1);

                List<Booking> bookings = _bookingsRepository.GetByDateAndPidForPeriod(period, currentPid, startDate);

                bookings.ForEach(x => bookingsDTO.Add(new BookingDTO()
                {
                    ID = x.ID,
                    EndDate = x.EndDate,
                    StartDate = x.StartDate,
                    Subject = x.Subject,
                    Owner = x.Owner,
                    IsSmartMeeting = x.IsSmartMeeting,
                    RecurrenceId = x.RecurrenceId,
                    ExternalAttendees = x.ExternalAttendees.Select(y =>
                    {
                        return new ExternalAttendeesDTO()
                        {
                            ID = y.ID,
                            BookingID = y.BookingID,
                            FullName = y.FullName,
                            CompanyName = y.CompanyName,
                            PassRequired = y.PassRequired
                        };
                    }),
                    Location = new LocationDTO()
                    {
                        ID = x.Room.Location.ID,
                        Name = x.Room.Location.Name,
                        LocationCredentials = x.Room.Location.LocationCredentials.ToList().Select(l => { return new LocationCredentialsDTO() { Department = l.Department, Email = l.Email, ID = l.ID, LocationID = l.LocationID, PhoneNumber = l.PhoneNumber }; }).ToList()
                    },
                    Room = new RoomDTO() { ID = x.Room.ID, RoomName = x.Room.RoomName, ComputerCount = x.Room.ComputerCount, PhoneCount = x.Room.PhoneCount, SmartRoom = x.Room.SmartRoom }
                }));
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get bookings for current user", ex);
            }
            return bookingsDTO;
        }

        /// <summary>
        /// API Endpoint to handle dynamic filters
        /// </summary>
        /// <param name="locationId">ID of location</param>
        /// <param name="startDate">Date of booking (any time)</param>
        /// <param name="room">Room</param>
        /// <param name="smartRoom">Smart meeting</param>
        /// <returns>List of bookings based on filters</returns>
        [HttpGet]
        [Route("search")]
        public IEnumerable<BookingDTO> GetBookingsFilterSearch(int? locationId, DateTime? startDate, string room, bool smartRoom)
        {
            string currentPid = User.Identity.Name.Substring(User.Identity.Name.IndexOf("\\") + 1);

            Location searchLocation = null;
            if (locationId != null && locationId != 0)
                searchLocation = _locationsRepository.GetLocationById(locationId.Value);

            Room searchRoom = null;
            if (room != null)
                searchRoom = _roomsRepository.GetRoomByName(room);

            var bookings = _bookingsRepository.GetByPartialDateRoomSmartLocationAndPid(startDate, DateTime.Now, currentPid, smartRoom, searchRoom, searchLocation);

            List<BookingDTO> bookingsDTO = new List<BookingDTO>();
            bookings.ForEach(x => bookingsDTO.Add(new BookingDTO()
            {
                ID = x.ID,
                EndDate = x.EndDate,
                StartDate = x.StartDate,
                Subject = x.Subject,
                Owner = x.Owner,
                IsSmartMeeting = x.IsSmartMeeting,
                RecurrenceId = x.RecurrenceId,
                ExternalAttendees = x.ExternalAttendees.Select(y =>
                {
                    return new ExternalAttendeesDTO()
                    {
                        ID = y.ID,
                        BookingID = y.BookingID,
                        FullName = y.FullName,
                        CompanyName = y.CompanyName,
                        PassRequired = y.PassRequired
                    };
                }),
                Location = new LocationDTO()
                {
                    ID = x.Room.Location.ID,
                    Name = x.Room.Location.Name,
                    LocationCredentials = x.Room.Location.LocationCredentials.ToList().Select(l => { return new LocationCredentialsDTO() { Department = l.Department, Email = l.Email, ID = l.ID, LocationID = l.LocationID, PhoneNumber = l.PhoneNumber }; }).ToList()
                },
                Room = new RoomDTO() { ID = x.Room.ID, RoomName = x.Room.RoomName, ComputerCount = x.Room.ComputerCount, PhoneCount = x.Room.PhoneCount, SmartRoom = x.Room.SmartRoom }
            }));

            return bookingsDTO;
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
                ExternalAttendees = new List<ExternalAttendees>() {  } ,// newBooking.ExternalAttendees,
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

        protected internal BookingDTO ConvertBookingToDTO(Booking clashedBooking)
        {
            return new BookingDTO()
            {
                ID = clashedBooking.ID,
                EndDate = clashedBooking.EndDate,
                StartDate = clashedBooking.StartDate,
                Subject = clashedBooking.Subject,
                Owner = clashedBooking.Owner,
                Location = new LocationDTO() { ID = clashedBooking.Room.Location.ID, Name = clashedBooking.Room.Location.Name },
                Room = new RoomDTO() { ID = clashedBooking.Room.ID, RoomName = clashedBooking.Room.RoomName, ComputerCount = clashedBooking.Room.ComputerCount, PhoneCount = clashedBooking.Room.PhoneCount, SmartRoom = clashedBooking.Room.SmartRoom }
            };
        }

        protected internal List<BookingDTO> ConvertBookingsToDTOs(List<Booking> clashedBookings)
        {
            List<BookingDTO> bookingsDTO = new List<BookingDTO>();
            clashedBookings.ToList().ForEach(x => bookingsDTO.Add(ConvertBookingToDTO(x)));

            return bookingsDTO;
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
    }
}