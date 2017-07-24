using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using VORBS.DAL.Repositories;
using VORBS.Models;

namespace VORBS.Services
{
    public class LocationsService
    {
        ILocationRepository _locationRepository;
        IBookingRepository _bookingRepository;
        IDirectoryService _directoryService;

        NLog.Logger _logger;

        public LocationsService(NLog.Logger logger, ILocationRepository locationRepository, IBookingRepository bookingRepository, IDirectoryService directoryService)
        {
            _logger = logger;
            _locationRepository = locationRepository;
            _bookingRepository = bookingRepository;
            _directoryService = directoryService;
        }

        public void EditLocation(Location originalLocation, Location editLocation)
        {
            if (editLocation.Name != originalLocation.Name)
            {
                bool exists = _locationRepository.GetLocationByName(editLocation.Name) != null;
                if (exists)
                    throw new LocationExistsException();
            }

            editLocation.ID = originalLocation.ID;

            _locationRepository.UpdateLocation(editLocation);
            _logger.Info(string.Format("Location {0} successfully editted", originalLocation.ID));
        }

        public void SaveNewLocation(Location newLocation)
        {
            var existingLocation = _locationRepository.GetLocationById(newLocation.ID);
            if (existingLocation == null)
            {
                _locationRepository.SaveNewLocation(newLocation);
                _logger.Info("New location successfully added: " + newLocation.Name + "/" + newLocation.ID);
            }
            else
                throw new LocationExistsException();
        }

        public void ToggleLocationActive(Location location, bool active, NameValueCollection appSettings)
        {
            location.Active = active;
            _locationRepository.UpdateLocation(location);
            
            if (!active)
            {
                List<Booking> bookings = _bookingRepository.GetByDateAndLocation(DateTime.Now, location)
                                            .OrderBy(b => b.Owner)
                                            .ToList();
                _bookingRepository.Delete(bookings);

                string fromEmail = appSettings["fromEmail"];

                if (bookings.Count() > 0)
                {
                    List<Booking> ownerBookings = new List<Booking>();
                    
                    string owner = bookings[0].Owner;

                    //Iterate around each booking until we hit a new subset belonging to a different owner. Then email that batch.
                    foreach (var booking in bookings)
                    {
                        if (booking.Owner != owner)
                        {
                            SendEmailToOwnerForMultiDelete(fromEmail, ownerBookings);

                            ownerBookings = new List<Booking>();
                            owner = booking.Owner;
                        }

                        ownerBookings.Add(booking);
                    }

                    //Final Send the last owner bookings
                    SendEmailToOwnerForMultiDelete(fromEmail, ownerBookings);
                }
            }
        }


        private void SendEmailToOwnerForMultiDelete(string fromEmail, List<Booking> ownerBookings)
        {
            try
            {
                string toEmail = _directoryService.GetUser(new User.Pid(ownerBookings[0].PID)).EmailAddress;
                string body = Utils.EmailHelper.GetEmailMarkup("~/Views/EmailTemplates/AdminMultiCancelledBooking.cshtml", ownerBookings);

                if (!string.IsNullOrEmpty(body) && !string.IsNullOrEmpty(toEmail))
                    Utils.EmailHelper.SendEmail(fromEmail, toEmail, "Meeting room booking(s) cancellation", body);
                else
                    throw new Exception("Body or To-Email is null.");
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to send admin multiple bookings email.", ex);
            }
        }

        public class LocationExistsException : Exception { }
    }
}