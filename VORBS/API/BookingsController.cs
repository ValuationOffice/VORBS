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
                    Location = new LocationDTO() { ID = x.Room.Location.ID, Name = x.Room.Location.Name },
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
                    Location = new LocationDTO() { ID = x.Room.Location.ID, Name = x.Room.Location.Name },
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
                    Location = new LocationDTO() { ID = x.Room.Location.ID, Name = x.Room.Location.Name },
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
            if (User.Identity.Name == null)
                return new List<BookingDTO>();

            List<BookingDTO> bookingsDTO = new List<BookingDTO>();

            try
            {
                string currentPid = User.Identity.Name.Substring(User.Identity.Name.IndexOf("\\") + 1); //TODO: Change?

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
                    Location = new LocationDTO() { ID = x.Room.Location.ID, Name = x.Room.Location.Name },
                    Room = new RoomDTO() { ID = x.Room.ID, RoomName = x.Room.RoomName, ComputerCount = x.Room.ComputerCount, PhoneCount = x.Room.PhoneCount, SmartRoom = x.Room.SmartRoom }
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
                    ExternalNames = booking.ExternalNames,
                    Flipchart = booking.Flipchart,
                    Projector = booking.Projector,
                    PID = booking.PID,
                    Location = new LocationDTO() { ID = booking.Room.Location.ID, Name = booking.Room.Location.Name },
                    Room = new RoomDTO() { ID = booking.Room.ID, RoomName = booking.Room.RoomName, ComputerCount = booking.Room.ComputerCount, PhoneCount = booking.Room.PhoneCount, SmartRoom = booking.Room.SmartRoom }
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
                newBooking.RoomID = newBooking.Room.ID;

                //Get the current user
                if (string.IsNullOrWhiteSpace(newBooking.PID))
                {
                    var user = AdQueries.GetUserByCurrentUser(User.Identity.Name);

                    if (user == null)
                        return Request.CreateResponse(HttpStatusCode.NotFound, "User not found in Active Directory. " + User.Identity.Name);

                    newBooking.Owner = user.Name;
                    newBooking.PID = user.SamAccountName;
                }

                //Reset Room as we dont want to create another room
                newBooking.Room = null;

                db.Bookings.Add(newBooking);
                db.SaveChanges();

                _logger.Info("Booking sucessfully created: " + newBooking.ID);

                string fromEmail = ConfigurationManager.AppSettings["fromEmail"];

                try
                {
                    string currentUserPid = User.Identity.Name.Substring(User.Identity.Name.IndexOf("\\") + 1);

                    string toEmail = AdQueries.GetUserByPid(newBooking.PID).EmailAddress;

                    string body = "";
                    if (newBooking.PID.ToUpper() != currentUserPid.ToUpper())
                        body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/AdminNewBooking.cshtml", newBooking);
                    else
                        body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/NewBooking.cshtml", newBooking);

                    Utils.EmailHelper.SendEmail(fromEmail, toEmail, "New booking confirmation", body);
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
                            Utils.EmailHelper.SendEmail(fromEmail, facilitiesEmail, "New booking requires facilities assistance", body);
                        }
                        catch (Exception ex)
                        {
                            _logger.ErrorException("Unable to send E-Mail to facilities for new booking: " + newBooking.ID, ex);
                        }
                    }
                }

                if (newBooking.ExternalNames != null)
                {
                    string securityEmail = bookingsLocation.LocationCredentials.Where(x => x.Department == LocationCredentials.DepartmentNames.security.ToString()).Select(x => x.Email).FirstOrDefault();
                    if (securityEmail != null)
                    {
                        try
                        {
                            string body = "";
                            body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/SecurityNewBooking.cshtml", newBooking);
                            Utils.EmailHelper.SendEmail(fromEmail, securityEmail, "New booking has external attendees", body);
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
                            Utils.EmailHelper.SendEmail(fromEmail, dssEmail, "New booking needs DSS assistnace", body);
                        }
                        catch (Exception ex)
                        {
                            _logger.ErrorException("Unable to send E-Mail to DSS for new booking: " + newBooking.ID, ex);
                        }
                    }
                }

                return new HttpResponseMessage(HttpStatusCode.OK);
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
                editBooking.ID = existingBookingId;

                //TODO: Maybe change when booking on behalf of user
                editBooking.Owner = existingBooking.Owner;
                editBooking.PID = existingBooking.PID;

                string body = "";
                string facilitiesEmail = "";
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
                        body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/FacilitiesEditeddBooking.cshtml", editBooking);
                    }
                }

                db.Entry(existingBooking).CurrentValues.SetValues(editBooking);
                db.SaveChanges();

                try
                {
                    //Send Dso Email
                    if (!string.IsNullOrEmpty(body))
                        Utils.EmailHelper.SendEmail(fromEmail, facilitiesEmail, "Edited booking requires facilities assistance", body);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Unable to send E-Mail to facilities for editting booking: " + editBooking.ID, ex);
                }

                //Send Owner Email
                try
                {
                    
                    string currentUserPid = User.Identity.Name.Substring(User.Identity.Name.IndexOf("\\") + 1);
                    string toEmail = AdQueries.GetUserByPid(editBooking.PID).EmailAddress;

                    if (editBooking.PID.ToUpper() != currentUserPid.ToUpper())
                        body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/AdminEdittedBooking.cshtml", editBooking);
                    else
                        body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/EdittedBooking.cshtml", editBooking);

                    Utils.EmailHelper.SendEmail(fromEmail, toEmail, "Booking edit confirmation", body);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Unable to send personal email for editting booking: " + editBooking.ID, ex);
                }

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

                db.Bookings.Remove(booking);
                db.SaveChanges();
                _logger.Info("Booking sucessfully cancelled: " + bookingId);

                string body = "";
                string fromEmail = ConfigurationManager.AppSettings["fromEmail"];

                try
                {
                    //Once Booking has been removed; Send Cancealtion Emails
                    
                    string currentUserPid = User.Identity.Name.Substring(User.Identity.Name.IndexOf("\\") + 1);

                    string toEmail = AdQueries.GetUserByPid(booking.PID).EmailAddress;

                    
                    if (booking.PID.ToUpper() != currentUserPid.ToUpper())
                        body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/AdminCancelledBooking.cshtml", booking);
                    else
                        body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/CancelledBooking.cshtml", booking);

                    Utils.EmailHelper.SendEmail(fromEmail, toEmail, "Booking cancellation confirmation", body);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Unable to send personal email for deleting booking: " + bookingId, ex);
                }


                if (booking.Flipchart || booking.Projector)
                {
                    Location bookingsLocation = db.Rooms.Where(x => x.ID == booking.RoomID).FirstOrDefault().Location;
                    string facilitiesEmail = bookingsLocation.LocationCredentials.Where(x => x.Department == LocationCredentials.DepartmentNames.facilities.ToString()).Select(x => x.Email).FirstOrDefault();
                    if (facilitiesEmail != null)
                    {
                        try
                        {
                            body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/FacilitiesDeletedBooking.cshtml", booking);
                            Utils.EmailHelper.SendEmail(fromEmail, facilitiesEmail, "Deleted booking requires facilities assistance", body);
                        }
                        catch (Exception ex)
                        {
                            _logger.ErrorException("Unable to send email to facilities for deleting booking: " + bookingId, ex);
                        }
                    }
                }

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.FatalException("Unable to delete booking: " + bookingId, ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("{owner}/{start:DateTime}")]
        [HttpGet]
        public List<BookingDTO> GetBookingByOwner(string owner, DateTime start)
        {
            List<BookingDTO> bookingsDTO = new List<BookingDTO>();
            try
            {
                List<Booking> bookings = db.Bookings
                    .Where(x => DbFunctions.TruncateTime(x.StartDate) == start.Date && x.Owner == owner).ToList()
                    .OrderBy(x => x.StartDate)
                    .ToList();

                bookings.ForEach(x => bookingsDTO.Add(new BookingDTO()
                {
                    ID = x.ID,
                    EndDate = x.EndDate,
                    StartDate = x.StartDate,
                    Subject = x.Subject,
                    Owner = x.Owner,
                    Location = new LocationDTO() { ID = x.Room.Location.ID, Name = x.Room.Location.Name },
                    Room = new RoomDTO() { ID = x.Room.ID, RoomName = x.Room.RoomName, ComputerCount = x.Room.ComputerCount, PhoneCount = x.Room.PhoneCount, SmartRoom = x.Room.SmartRoom }
                }));

                return bookingsDTO;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get list of bookings for owner: " + owner, ex);
            }
            return bookingsDTO;
        }
    }
}
