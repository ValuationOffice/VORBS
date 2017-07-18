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
        private BookingsService _bookingService;

        public BookingsController(IBookingRepository bookingRepository, ILocationRepository locationRepository, IRoomRepository roomsRepository, IDirectoryService directoryService)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();

            _directoryService = directoryService;
            _bookingService = new BookingsService(_logger, bookingRepository, roomsRepository, locationRepository, directoryService);

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
            try
            {
                User currentUser = _directoryService.GetCurrentUser(User.Identity.Name);
                if (currentUser == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound, "User not found in Active Directory. " + User.Identity.Name);

                _bookingService.SaveNewBooking(newBooking, currentUser, ConfigurationManager.AppSettings);
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

            try
            {
                User currentUser = _directoryService.GetCurrentUser(User.Identity.Name);
                if (currentUser == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound, "User not found in Active Directory. " + User.Identity.Name);

                _bookingService.EditExistingBooking(existingBooking, editBooking, recurrence, currentUser, ConfigurationManager.AppSettings);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (UnableToEditBookingException e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "An error occured whilst updating the booking(s). Please try again or contact Service Desk");
            }
            catch (DAL.BookingConflictException ex)
            {
                _logger.FatalException("Unable to edit booking: " + editBooking.Owner + "/ id: " + editBooking.ID, ex);
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.FatalException("Unable to edit booking: " + editBooking.Owner + "/ id: " + editBooking.ID, ex);
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

                User currentUser = _directoryService.GetCurrentUser(User.Identity.Name);
                if (currentUser == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound, "User not found in Active Directory. " + User.Identity.Name);

                _bookingService.DeleteExistingBooking(booking, recurrence, currentUser, ConfigurationManager.AppSettings);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (DeletionException e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Unable to delete existing booking. An error occured, please contact help desk.");
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