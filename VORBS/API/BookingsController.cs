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

namespace VORBS.API
{
    [RoutePrefix("api/bookings")]
    public class BookingsController : ApiController
    {
        private NLog.Logger _logger;
        private VORBSContext db = new VORBSContext();

        public BookingsController()
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
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
                List<Booking> bookings = db.Bookings
                    .Where(x => x.StartDate >= start && x.EndDate <= end && x.Room.Location.Name == location)
                    .ToList();


                bookings.ForEach(x => bookingsDTO.Add(new BookingDTO()
                {
                    ID = x.ID,
                    EndDate = x.EndDate,
                    StartDate = x.StartDate,
                    Owner = x.Owner,
                    IsSmartMeeting = x.IsSmartMeeting,
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
                List<Booking> bookings = db.Bookings
                    .Where(x => x.StartDate >= start && x.EndDate <= end && x.Room.Location.Name == location && x.Room.RoomName == room)
                    .ToList();

                bookings.ForEach(x => bookingsDTO.Add(new BookingDTO()
                {
                    ID = x.ID,
                    EndDate = x.EndDate,
                    StartDate = x.StartDate,
                    Owner = x.Owner,
                    IsSmartMeeting = x.IsSmartMeeting,
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
                List<Booking> bookings = db.Bookings
                    .Where(x => x.Owner == person && x.StartDate >= start && x.EndDate <= end && x.Room.Location.Name == location && x.Room.RoomName == room)
                    .ToList();


                bookings.ForEach(x => bookingsDTO.Add(new BookingDTO()
                {
                    ID = x.ID,
                    EndDate = x.EndDate,
                    StartDate = x.StartDate,
                    Owner = x.Owner,
                    IsSmartMeeting = x.IsSmartMeeting,
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
                string currentPid = (AdQueries.IsOffline()) ? "localuser" : User.Identity.Name.Substring(User.Identity.Name.IndexOf("\\") + 1);

                List<Booking> bookings = db.Bookings
                    .Where(x => x.PID == currentPid && x.EndDate >= start).ToList()
                    .OrderBy(x => x.StartDate)
                    .ToList();

                bookings.ForEach(x => bookingsDTO.Add(new BookingDTO()
                {
                    ID = x.ID,
                    EndDate = x.EndDate,
                    StartDate = x.StartDate,
                    Subject = x.Subject,
                    Owner = x.Owner,
                    IsSmartMeeting = x.IsSmartMeeting,
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
                Booking booking = db.Bookings.Single(b => b.ID == bookingId);

                bookingsDTO = new BookingDTO()
                {
                    ID = booking.ID,
                    EndDate = booking.EndDate,
                    StartDate = booking.StartDate,
                    Subject = booking.Subject,
                    Owner = booking.Owner,
                    NumberOfAttendees = booking.NumberOfAttendees,
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
            try
            {
                List<Booking> bookingsToCreate = new List<Booking>();
                List<Booking> clashedBookings = new List<Booking>();
                List<Booking> deletedBookings = new List<Booking>();

                List<DateTime> recurringDates = new List<DateTime>();

                Room bookingRoom = db.Rooms.Where(x => x.ID == newBooking.RoomID).FirstOrDefault();
                newBooking.RoomID = newBooking.Room.ID;

                bool doMeetingsClash = false;

                //Get the current user
                if (string.IsNullOrWhiteSpace(newBooking.PID) || string.IsNullOrWhiteSpace(newBooking.Owner))
                {
                    var user = (AdQueries.IsOffline()) ? AdQueries.CreateFakeUser() : AdQueries.GetUserByCurrentUser(User.Identity.Name);

                    if (user == null)
                        return Request.CreateResponse(HttpStatusCode.NotFound, "User not found in Active Directory. " + User.Identity.Name);

                    newBooking.Owner = user.Name;
                    newBooking.PID = user.SamAccountName;
                }

                if (newBooking.Recurrence.IsRecurring)
                {
                    AvailabilityController aC = new AvailabilityController();

                    recurringDates = GetDatesForRecurrencePeriod(newBooking.StartDate, newBooking.Recurrence);

                    if (newBooking.SmartLoactions.Count() > 0 && newBooking.Room.SmartRoom)
                    {
                        var smartBookings = GetSmartRoomBookings(newBooking, out clashedBookings);

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
                                    int[] ids = clashedBookings.Select(x => x.ID).ToArray();
                                    var entityClashedBookings = db.Bookings.Where(x => ids.Contains(x.ID));
                                    db.Bookings.RemoveRange(entityClashedBookings);
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
                    var smartBookings = GetSmartRoomBookings(newBooking, out clashedBookings);

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

                db.Bookings.AddRange(bookingsToCreate);
                db.ExternalAttendees.AddRange(newBooking.ExternalAttendees);

                db.SaveChanges(bookingsToCreate, true);

                if (deletedBookings.Count > 0)
                {
                    foreach (string owner in deletedBookings.Select(x => x.PID).Distinct())
                    {
                        try
                        {
                            //Once Booking has been removed; Send Cancelltion Emails
                            string toEmail = AdQueries.GetUserByPid(owner).EmailAddress;
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

                _logger.Info("Booking sucessfully created: " + newBooking.ID);

                if (AdQueries.IsOffline())
                    return new HttpResponseMessage(HttpStatusCode.OK);

                string fromEmail = ConfigurationManager.AppSettings["fromEmail"];
                newBooking.Room = db.Rooms.Where(x => x.ID == newBooking.RoomID).FirstOrDefault();
                try
                {
                    string currentUserPid = User.Identity.Name.Substring(User.Identity.Name.IndexOf("\\") + 1);

                    string toEmail = AdQueries.GetUserByPid(newBooking.PID).EmailAddress;

                    string body = "";

                    if (newBooking.IsSmartMeeting)
                        bookingsToCreate = GetRoomForBookings(bookingsToCreate);
                    else
                        newBooking.Room = GetRoomForBooking(newBooking);


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
                Location bookingsLocation = db.Rooms.Where(x => x.ID == newBooking.RoomID).FirstOrDefault().Location;
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
        [Route("{existingBookingId:int}")]
        public HttpResponseMessage EditExistingBooking(int existingBookingId, Booking editBooking)
        {
            try
            {
                //Find Existing Booking
                Booking existingBooking = db.Bookings.Single(b => b.ID == existingBookingId);

                //Check to see if booking has changed
                if (!BookingHasChanged(editBooking, existingBooking))
                    return Request.CreateResponse(HttpStatusCode.NotModified, "Booking has not changed.");

                editBooking.ID = existingBookingId;

                editBooking.Owner = existingBooking.Owner;
                editBooking.PID = existingBooking.PID;
                editBooking.Room = db.Rooms.SingleOrDefault(x => x.ID == editBooking.RoomID);

                string body = "";
                string dssBody = "";

                string facilitiesEmail = "";
                string dssEmail = "";

                string fromEmail = ConfigurationManager.AppSettings["fromEmail"];

                _logger.Info("Booking sucessfully editted: " + editBooking.ID);

                //Send DSO Email
                //SendDSOEmail(dsoEmailMessage);
                //TODO: Refactor
                ////Create DSO Email but do not send until db.savechanges
                Location bookingsLocation = db.Rooms.Where(x => x.ID == editBooking.RoomID).FirstOrDefault().Location;
                if ((existingBooking.Flipchart != editBooking.Flipchart) || (editBooking.Projector != existingBooking.Projector))
                {
                    facilitiesEmail = bookingsLocation.LocationCredentials.Where(x => x.Department == LocationCredentials.DepartmentNames.facilities.ToString()).Select(x => x.Email).FirstOrDefault();
                    if (facilitiesEmail != null)
                    {
                        
                        List<string> addedEquipment = new List<string>();
                        List<string> removedEquipment = new List<string>();
                        if (editBooking.Projector != existingBooking.Projector)
                        {
                            //If it is present in edit booking, and above equality check is false, then we know the projector request is new
                            if (editBooking.Projector)
                                addedEquipment.Add("Projector");
                            else
                                removedEquipment.Add("Projector");
                        }
                        if (editBooking.Flipchart != existingBooking.Flipchart)
                        {
                            if (editBooking.Flipchart)
                                addedEquipment.Add("Flip Chart");
                            else
                                removedEquipment.Add("Flip Chart");
                        }
                        try
                        {
                            
                            Models.ViewModels.facilitiesEdittedBooking viewModel = new Models.ViewModels.facilitiesEdittedBooking() { EdittedBooking = editBooking, OriginalBooking = existingBooking, EquipmentAdded = addedEquipment, EquipmentRemoved = removedEquipment };
                            body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/FacilitiesEdittedBooking.cshtml", viewModel);
                        }
                        catch (Exception exn)
                        {
                            _logger.ErrorException("Unable to retrieve email markup for facilities for editted booking: " + editBooking.ID, exn);
                        }                        
                    }
                }

                if (editBooking.DssAssist && !existingBooking.DssAssist)
                {
                    dssEmail = bookingsLocation.LocationCredentials.Where(x => x.Department == LocationCredentials.DepartmentNames.dss.ToString()).Select(x => x.Email).FirstOrDefault();
                    if (dssEmail != null)
                    {
                        try
                        {
                            editBooking.Room.Location = existingBooking.Room.Location;
                            dssBody = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/DSSEdittedBooking.cshtml", editBooking);
                        }
                        catch (Exception ex)
                        {
                            _logger.ErrorException("Unable to retrieve E-Mail markup for DSS for new booking: " + editBooking.ID, ex);
                        }
                    }
                }
                string securityEmailBody = "";
                string securityEmail = "";
                if (!(new HashSet<string>(existingBooking.ExternalAttendees.Select(x => x.FullName)).SetEquals(editBooking.ExternalAttendees.Select(x => x.FullName))))
                {

                    try
                    {
                        securityEmail = bookingsLocation.LocationCredentials.Where(x => x.Department == LocationCredentials.DepartmentNames.security.ToString()).Select(x => x.Email).FirstOrDefault();
                        IEnumerable<ExternalAttendees> removedAttendees = existingBooking.ExternalAttendees.Where(x => !editBooking.ExternalAttendees.Select(y => y.FullName).Contains(x.FullName));
                        IEnumerable<ExternalAttendees> addedAttendees = editBooking.ExternalAttendees.Where(x => !existingBooking.ExternalAttendees.Select(y => y.FullName).Contains(x.FullName));
                        securityEmailBody = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/SecurityEdittedBooking.cshtml", new Models.ViewModels.SecurityEdittedBooking() { EdittedBooking = editBooking, NewAttendees = addedAttendees, RemovedAttendees = removedAttendees });
                    }
                    catch (Exception exn)
                    {
                        _logger.ErrorException("Unable to retrieve security email markup for editting booking: " + editBooking.ID, exn);
                    }
                }

                db.Entry(existingBooking).CurrentValues.SetValues(editBooking);
                db.ExternalAttendees.RemoveRange(db.ExternalAttendees.Where(x => x.BookingID == editBooking.ID));

                if (editBooking.ExternalAttendees != null)
                {
                    editBooking.ExternalAttendees.ToList().ForEach(x => x.BookingID = editBooking.ID);
                    db.ExternalAttendees.AddRange(editBooking.ExternalAttendees);
                }

                db.SaveChanges();

                _logger.Info("Booking sucessfully editted: " + editBooking.ID);

                if (AdQueries.IsOffline())
                    return new HttpResponseMessage(HttpStatusCode.OK);

                try
                {
                    //Send facilities Email
                    if (!string.IsNullOrEmpty(body))
                        Utils.EmailHelper.SendEmail(fromEmail, facilitiesEmail, string.Format("(Updated) Meeting room equipment for booking on {0}", editBooking.StartDate.ToShortDateString()), body);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Unable to send E-Mail to facilities for editting booking: " + editBooking.ID, ex);
                }


                try
                {
                    //Send DSS Email
                    if (!string.IsNullOrEmpty(dssBody))
                        Utils.EmailHelper.SendEmail(fromEmail, dssEmail, string.Format("(Updated) SMART room set up support on {0}", editBooking.StartDate.ToShortDateString()), dssBody);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Unable to send E-Mail to DSS for editting booking: " + editBooking.ID, ex);
                }

                try
                {
                    if (!string.IsNullOrEmpty(securityEmailBody) && !string.IsNullOrEmpty(securityEmail))
                        Utils.EmailHelper.SendEmail(fromEmail, securityEmail, "(Updated) External guests notification for " + editBooking.StartDate.ToShortDateString(), securityEmailBody);
                }
                catch (Exception exn)
                {
                    _logger.ErrorException("Unable to send E-Mail to security for editting booking: " + editBooking.ID, exn);
                }

                Booking edittedBooking = db.Bookings.Single(b => b.ID == existingBooking.ID);

                //Send Owner Email
                try
                {
                    string currentUserPid = User.Identity.Name.Substring(User.Identity.Name.IndexOf("\\") + 1);
                    string toEmail = AdQueries.GetUserByPid(editBooking.PID).EmailAddress;

                    if (editBooking.PID.ToUpper() != currentUserPid.ToUpper())
                    {
                        body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/AdminEdittedBooking.cshtml", edittedBooking);
                        Utils.EmailHelper.SendEmail(fromEmail, toEmail, "Meeting room edit confirmation", body);
                    }
                    else
                    {
                        body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/EdittedBooking.cshtml", edittedBooking);
                        Utils.EmailHelper.SendEmail(fromEmail, toEmail, "Meeting room booking confirmation", body);
                    }


                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Unable to send personal email for editting booking: " + editBooking.ID, ex);
                }

                //check if the attendees are different

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.FatalException("Unable to edit booking: " + editBooking.ID, ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("{bookingId:Int}")]
        [HttpDelete]
        public HttpResponseMessage DeleteBookingById(int bookingId)
        {
            try
            {
                Booking booking = db.Bookings.First(b => b.ID == bookingId);
                Location bookingsLocation = db.Rooms.Where(x => x.ID == booking.RoomID).FirstOrDefault().Location;

                string fromEmail = ConfigurationManager.AppSettings["fromEmail"];

                //NOT NEEDED RIGHT NOW, BUT WILL SEND SECURITY E-MAILS WHEN NEEDED.
                //string securityEmailBody = "";
                //if (booking.ExternalAttendees.Count > 0)
                //{
                //    securityEmailBody = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/SecurityDeletedBooking.cshtml", booking);
                //}

                db.Bookings.Remove(booking);
                db.SaveChanges();

                _logger.Info("Booking sucessfully cancelled: " + bookingId);

                if (AdQueries.IsOffline())
                    return new HttpResponseMessage(HttpStatusCode.OK);

                Room locationRoom = db.Rooms.First(b => b.ID == booking.RoomID);
                booking.Room = locationRoom;

                string body = "";


                try
                {
                    //Once Booking has been removed; Send Cancelltion Emails

                    string currentUserPid = User.Identity.Name.Substring(User.Identity.Name.IndexOf("\\") + 1);

                    string toEmail = AdQueries.GetUserByPid(booking.PID).EmailAddress;


                    if (booking.PID.ToUpper() != currentUserPid.ToUpper())
                        body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/AdminCancelledBooking.cshtml", booking);
                    else
                        body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/CancelledBooking.cshtml", booking);

                    Utils.EmailHelper.SendEmail(fromEmail, toEmail, "Meeting room booking cancellation", body);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Unable to send personal email for deleting booking: " + bookingId, ex);
                }


                if (booking.Flipchart || booking.Projector)
                {

                    string facilitiesEmail = bookingsLocation.LocationCredentials.Where(x => x.Department == LocationCredentials.DepartmentNames.facilities.ToString()).Select(x => x.Email).FirstOrDefault();
                    if (facilitiesEmail != null)
                    {
                        try
                        {
                            body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/FacilitiesDeletedBooking.cshtml", booking);
                            Utils.EmailHelper.SendEmail(fromEmail, facilitiesEmail, string.Format("(Updated) Meeting room equipment for booking on {0}", booking.StartDate.ToShortDateString()), body);
                        }
                        catch (Exception ex)
                        {
                            _logger.ErrorException("Unable to send email to facilities for deleting booking: " + bookingId, ex);
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
                            body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/DSSDeletedBooking.cshtml", booking);
                            Utils.EmailHelper.SendEmail(fromEmail, dssEmail, string.Format("(Updated) SMART room set up support on {0}", booking.StartDate.ToShortDateString()), body);
                        }
                        catch (Exception ex)
                        {
                            _logger.ErrorException("Unable to send email to dss for deleting booking: " + bookingId, ex);
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
            //Building up a string of Lambda C# string (Using Linq.Dynamic) so we can on the fly change the where clause based on the concatanation
            string queryExpression = "";

            try
            {
              //Filter by owner
                queryExpression += (owner == null || owner.Equals("")) ? "" : (queryExpression.Length > 0) ? " AND OWNER = \"" + owner + "\"" : "OWNER = \"" + owner + "\"";

                //Standard pattern used all below, if the value is null to start, we wont filter on it if it isnt null, check if the string is empty already. If the string is not empty, start with the AND keyword as we are concatanating on otherwise start with your expression as it is the begging of the expression
                queryExpression += (location == null || location == 0) ? "" : (queryExpression.Length > 0) ? " AND Room.LocationID = " + location : "Room.LocationID = " + location;
                
                // Filter by room

                if (!string.IsNullOrEmpty(room))
                {
                    int roomID = 0;
                    roomID = db.Bookings.Where(x => x.Room.RoomName == room).Select(x => x.RoomID).ToList()[0];
                    queryExpression += (queryExpression.Length > 0) ? " AND Room.ID = " + roomID : "Room.ID = " + roomID;
                }

                if (start != null)
                {
                    //Need to get 00:00 and 23:59 times for the day as this allows us to get any booking during the day. We cant use .toshortdatestring etc inside a linq expression, even when not using dynamic linq
                    DateTime startDate00Hours = DateTime.Parse(DateTime.Parse(start.ToString()).ToShortDateString());
                    //Need to do a format string which looks like " DateTime(YEAR, MONTH, DAY, HOUR, MINUTE) " so that we can parse it in the expression and let Linq interperite it as a datetime creation
                    string startDate00HoursFormatted = "DateTime(" + startDate00Hours.Year + ", " + startDate00Hours.Month + ", " + startDate00Hours.Day + ", " + startDate00Hours.Hour + ", " + startDate00Hours.Minute + ", " + startDate00Hours.Second + ")";
                    DateTime startDate2359Hours = DateTime.Parse(DateTime.Parse(start.ToString()).ToShortDateString()).AddHours(23).AddMinutes(59).AddSeconds(59);
                    string startDate2359HoursFormatted = "DateTime(" + startDate2359Hours.Year + ", " + startDate2359Hours.Month + ", " + startDate2359Hours.Day + ", " + startDate2359Hours.Hour + ", " + startDate2359Hours.Minute + ", " + startDate2359Hours.Second + ")";
                    queryExpression += (start == null) ? "" : (queryExpression.Length > 0) ? " AND StartDate >= " + startDate00HoursFormatted + " AND EndDate <= " + startDate2359HoursFormatted + "" : " StartDate >= " + startDate00HoursFormatted + " AND EndDate <= " + startDate2359HoursFormatted;
                }
                
            }
            catch (Exception exn)
            {
                queryExpression = "";
                _logger.ErrorException("Unable to create query expression for filter in bookings", exn);
            }


            //If expression is empty, dont try and filter on it .. where() does not work.
            if (queryExpression.Contains("=")) 
            {
                try
                {
                    //Filter on the expression we built as if it was written in C# POCO
                    var bookings = db.Bookings.Where(queryExpression).ToList();

                    List<BookingDTO> bookingsDTO = new List<BookingDTO>();
                    bookings.ForEach(x => bookingsDTO.Add(new BookingDTO()
                    {
                        ID = x.ID,
                        EndDate = x.EndDate,
                        StartDate = x.StartDate,
                        Subject = x.Subject,
                        Owner = x.Owner,
                        IsSmartMeeting = x.IsSmartMeeting,
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
                catch (Exception exn)
                {
                    _logger.ErrorException("Unable to filter bookings based on query expression", exn);
                }

            }
                    
            //If we didnt filter on anything, then return nothing
            return new List<BookingDTO>();
        }

        [HttpGet]
        [Route("{startDate:DateTime}/{period:int}")]
        public IEnumerable<BookingDTO> GetBookingsForPeriod(DateTime startDate, int period)
        {
            List<BookingDTO> bookingsDTO = new List<BookingDTO>();

            try
            {
                string currentPid = (AdQueries.IsOffline()) ? "localuser" : User.Identity.Name.Substring(User.Identity.Name.IndexOf("\\") + 1);

                DateTime endDuration = startDate.AddDays(period - 1);
                endDuration = DateTime.Parse(endDuration.ToShortDateString()).AddHours(23).AddMinutes(59).AddSeconds(59);

                List<Booking> bookings = db.Bookings
                    .Where(x => x.PID == currentPid && x.EndDate >= startDate && x.EndDate <= endDuration).ToList()
                    .OrderBy(x => x.StartDate)
                    .ToList();

                bookings.ForEach(x => bookingsDTO.Add(new BookingDTO()
                {
                    ID = x.ID,
                    EndDate = x.EndDate,
                    StartDate = x.StartDate,
                    Subject = x.Subject,
                    Owner = x.Owner,
                    IsSmartMeeting = x.IsSmartMeeting,
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

            //Building up a string of Lambda C# string (Using Linq.Dynamic) so we can on the fly change the where clause based on the concatanation
            string queryExpression = "";

            try
            {
                string currentPid = (AdQueries.IsOffline()) ? "localuser" : User.Identity.Name.Substring(User.Identity.Name.IndexOf("\\") + 1);

                //Filter by users pay id
                queryExpression += (currentPid == null || currentPid == "") ? "" : (queryExpression.Length > 0) ? " AND PID = \"" + currentPid + "\"" : "PID = \"" + currentPid + "\"";

                //Standard pattern used all below, if the value is null to start, we wont filter on it if it isnt null, check if the string is empty already. If the string is not empty, start with the AND keyword as we are concatanating on otherwise start with your expression as it is the begging of the expression
                queryExpression += (locationId == null || locationId == 0) ? "" : (queryExpression.Length > 0) ? " AND Room.LocationID = " + locationId : "Room.LocationID = " + locationId;

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
                else
                {
                    DateTime now = DateTime.Now;
                    string nowFormatted = "DateTime(" + now.Year + ", " + now.Month + ", " + now.Day + ", " + now.Hour + ", " + now.Minute + ", " + now.Second + ")";
                    //queryExpression += (startDate == null) ? "" : (queryExpression.Length > 0) ? " AND StartDate >= " + startDate00HoursFormatted + " AND EndDate <= " + startDate2359HoursFormatted + "" : " StartDate >= " + startDate00HoursFormatted + " AND EndDate <= " + startDate2359HoursFormatted;
                    queryExpression += (queryExpression.Length > 0) ? " AND EndDate >= " + nowFormatted : "EndDate >= " + nowFormatted;
                }

                queryExpression += (room == null) ? "" : (queryExpression.Length > 0) ? " AND Room.RoomName = \"" + room + "\"" : "Room.RoomName = \"" + room + "\"";
                queryExpression += (!smartRoom) ? "" : (queryExpression.Length > 0) ? " AND IsSmartMeeting = " + smartRoom : "IsSmartMeeting = " + smartRoom;
            }
            catch (Exception exn)
            {
                queryExpression = "";
                _logger.ErrorException("Unable to create query expression for filter in bookings", exn);
            }


            //If expression is empty, dont try and filter on it .. where() does not work.
            if (queryExpression.Trim().Length > 0)
            {
                try
                {
                    //Filter on the expression we built as if it was written in C# POCO
                    var bookings = db.Bookings.Where(queryExpression).ToList();

                    List<BookingDTO> bookingsDTO = new List<BookingDTO>();
                    bookings.ForEach(x => bookingsDTO.Add(new BookingDTO()
                    {
                        ID = x.ID,
                        EndDate = x.EndDate,
                        StartDate = x.StartDate,
                        Subject = x.Subject,
                        Owner = x.Owner,
                        IsSmartMeeting = x.IsSmartMeeting,
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
                catch (Exception exn)
                {
                    _logger.ErrorException("Unable to filter bookings based on query expression", exn);
                }

            }
            //If we didnt filter on anything, then return nothing
            return new List<BookingDTO>();
        }

        private Room GetRoomForBooking(Booking newBooking)
        {
            return db.Rooms.Single(r => r.ID == newBooking.RoomID);
        }

        protected internal List<Booking> GetRoomForBookings(List<Booking> bookingsToCreate)
        {
            bookingsToCreate.ForEach(b => b.Room = GetRoomForBooking(b));
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

        protected internal List<Booking> GetSmartRoomBookings(Booking newBooking, out List<Booking> clashedBookings)
        {
            List<Booking> bookingsToCreate = new List<Booking>();
            List<Booking> clashedBs = new List<Booking>();
            List<int> smartRoomIds = new List<int>();

            smartRoomIds.Add(newBooking.RoomID);

            AvailabilityController aC = new AvailabilityController();

            foreach (var smartLoc in newBooking.SmartLoactions)
            {
                Room smartRoom = aC.GetAlternateSmartRoom(smartRoomIds, newBooking.StartDate, newBooking.EndDate, db.Locations.Single(l => l.Name == smartLoc).ID);

                if (smartRoom == null || bookingsToCreate.Select(x => x.Room).Contains(smartRoom))
                {
                    clashedBs.Add(new Booking()
                    {
                        StartDate = newBooking.StartDate,
                        Owner = newBooking.Owner,
                        IsSmartMeeting = true,
                        Room = new Room()
                        {
                            Location = new Location()
                            {
                                Name = smartLoc
                            }
                        }
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
                        RoomID = smartRoom.ID,
                        Room = smartRoom,
                        Subject = newBooking.Subject,
                        StartDate = newBooking.StartDate,
                        EndDate = newBooking.EndDate,
                        IsSmartMeeting = true
                    });

                    smartRoomIds.Add(smartRoom.ID);
                }
            }

            clashedBookings = clashedBs;
            return bookingsToCreate;
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

        protected internal bool BookingHasChanged(Booking editBooking, Booking existingBooking)
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