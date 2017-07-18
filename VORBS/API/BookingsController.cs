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
using static VORBS.Services.BookingsService;

namespace VORBS.API
{
    [RoutePrefix("api/bookings")]
    public class BookingsController : ApiController
    {
        private NLog.Logger _logger;
        private IBookingRepository _bookingsRepository;
        private ILocationRepository _locationsRepository;
        private IRoomRepository _roomsRepository;

        private IDirectoryService _directoryService;

        public BookingsController(IBookingRepository bookingRepository, ILocationRepository locationRepository, IRoomRepository roomsRepository, IDirectoryService directoryService)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();

            _directoryService = directoryService;

            _bookingsRepository = bookingRepository;
            _locationsRepository = locationRepository;
            _roomsRepository = roomsRepository;
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
            BookingsService bookingService = new BookingsService(_logger, _bookingsRepository, _roomsRepository, _locationsRepository, _directoryService);

            try
            {
                User currentUser = new User();

                if (string.IsNullOrWhiteSpace(newBooking.PID) || string.IsNullOrWhiteSpace(newBooking.Owner))
                {
                    currentUser = _directoryService.GetCurrentUser(User.Identity.Name);

                    if (currentUser == null)
                        return Request.CreateResponse(HttpStatusCode.NotFound, "User not found in Active Directory. " + User.Identity.Name);
                }

                bookingService.SaveNewBooking(newBooking, currentUser, ConfigurationManager.AppSettings);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (BookingsService.BookingConflictException e)
            {
                var clashedBookingsString = new JavaScriptSerializer().Serialize(ConvertBookingsToDTOs(e.clashedBookings));
                return Request.CreateErrorResponse(HttpStatusCode.Conflict, clashedBookingsString);
            }
            catch (DAL.BookingConflictException e)
            {
                _logger.FatalException("Unable to save new booking: " + newBooking.Owner + "/" + newBooking.StartDate, e);
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
            catch (ClashedBookingsException e)
            {
                var clashedBookingsString = new JavaScriptSerializer().Serialize(ConvertBookingsToDTOs(e.clashedBookings));
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway, clashedBookingsString);
            }
            catch (UnauthorisedOverwriteException e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Unauthorised to overwrite bookings.");
            }
            catch (Exception e)
            {
                _logger.FatalException("Unable to save new booking: " + newBooking.Owner + "/" + newBooking.StartDate, e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
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
            catch (DAL.BookingConflictException ex)
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


        protected internal List<BookingDTO> ConvertBookingsToDTOs(List<Booking> clashedBookings)
        {
            List<BookingDTO> bookingsDTO = new List<BookingDTO>();
            clashedBookings.ToList().ForEach(x => bookingsDTO.Add(ConvertBookingToDTO(x)));

            return bookingsDTO;
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

    }
}