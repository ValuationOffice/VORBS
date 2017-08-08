using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VORBS.DAL.Repositories;
using VORBS.Models;
using VORBS.Services;
using static VORBS.Models.User;
using static VORBS.Services.AdminService;
using static VORBS.Services.LocationsService;

namespace VORBS.Tests.Services
{
    [TestClass]
    public class LocationsServiceTests : BaseServiceTestSetup
    {
        [TestMethod]
        public void EditLocation_Should_Edit()
        {
            Location existingLocation = new Location()
            {
                ID = 1,
                Name = "Location1",
                Active = true,
                AdditionalInformation = "Original additional info"
            };

            Location edittedLocation = new Location()
            {
                ID = 1,
                Name = "Location1",
                Active = true,
                AdditionalInformation = "Editted additional info"
            };

            mockLocationRepository.Setup(m => m.UpdateLocation(edittedLocation));

            LocationsService locationService = new LocationsService(logger.Object, mockLocationRepository.Object, mockBookingRepository.Object, mockDirectoryRepository.Object, mockEmailHelper.Object);
            locationService.EditLocation(existingLocation, edittedLocation);

            mockLocationRepository.Verify(m => m.UpdateLocation(edittedLocation), Times.Once);
        }

        [TestMethod]
        public void EditLocation_Should_ThrowException_If_LocationExists()
        {
            Location existingLocation = new Location()
            {
                ID = 1,
                Name = "Location1",
                Active = true,
                AdditionalInformation = "Original additional info"
            };

            Location edittedLocation = new Location()
            {
                ID = 1,
                Name = "Location2",
                Active = true,
                AdditionalInformation = "Editted additional info"
            };

            mockLocationRepository.Setup(m => m.GetLocationByName(edittedLocation.Name)).Returns(edittedLocation);
            mockLocationRepository.Setup(m => m.UpdateLocation(edittedLocation));

            LocationsService locationService = new LocationsService(logger.Object, mockLocationRepository.Object, mockBookingRepository.Object, mockDirectoryRepository.Object, mockEmailHelper.Object);

            Assert.ThrowsException<LocationExistsException>(() => locationService.EditLocation(existingLocation, edittedLocation));

            mockLocationRepository.Verify(m => m.UpdateLocation(edittedLocation), Times.Never);
        }

        [TestMethod]
        public void SaveNewLocation_Should_SaveLocation()
        {
            Location newLocation = new Location()
            {
                ID = 1,
                Name = "Location1",
                Active = true,
                AdditionalInformation = "Original additional info"
            };

            mockLocationRepository.Setup(m => m.GetLocationById(newLocation.ID)).Returns((Location)null);
            mockLocationRepository.Setup(m => m.SaveNewLocation(newLocation));

            LocationsService locationService = new LocationsService(logger.Object, mockLocationRepository.Object, mockBookingRepository.Object, mockDirectoryRepository.Object, mockEmailHelper.Object);
            locationService.SaveNewLocation(newLocation);

            mockLocationRepository.Verify(m => m.SaveNewLocation(newLocation), Times.Once);
        }

        [TestMethod]
        public void SaveNewLocation_Should_ThrowException_When_LocationExists()
        {
            Location newLocation = new Location()
            {
                ID = 1,
                Name = "Location1",
                Active = true,
                AdditionalInformation = "Original additional info"
            };

            mockLocationRepository.Setup(m => m.GetLocationById(newLocation.ID)).Returns(newLocation);
            mockLocationRepository.Setup(m => m.SaveNewLocation(newLocation));

            LocationsService locationService = new LocationsService(logger.Object, mockLocationRepository.Object, mockBookingRepository.Object, mockDirectoryRepository.Object, mockEmailHelper.Object);
            Assert.ThrowsException<LocationExistsException>(() => locationService.SaveNewLocation(newLocation));

            mockLocationRepository.Verify(m => m.SaveNewLocation(newLocation), Times.Never);
        }

        [TestMethod]
        public void ToggleLocationActive_Should_Update_When_True()
        {
            Location existingLocation = new Location()
            {
                ID = 1,
                Name = "Location1",
                Active = false,
                AdditionalInformation = "Original additional info"
            };

            NameValueCollection appSettings = new NameValueCollection();
            appSettings.Add("fromEmail", "anyemail@email.com");

            LocationsService service = new LocationsService(logger.Object, mockLocationRepository.Object, mockBookingRepository.Object, mockDirectoryRepository.Object, mockEmailHelper.Object);
            service.ToggleLocationActive(existingLocation, true, appSettings);

            mockLocationRepository.Verify(m => m.UpdateLocation(existingLocation), Times.Once);
            mockBookingRepository.Verify(m => m.Delete(It.IsAny<List<Booking>>()), Times.Never);
            mockEmailHelper.Verify(m => m.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        [TestMethod]
        public void ToggleLocationActive_Should_Update_When_False_And_NoExistingBookings()
        {
            Location existingLocation = new Location()
            {
                ID = 1,
                Name = "Location1",
                Active = true,
                AdditionalInformation = "Original additional info"
            };

            mockBookingRepository.Setup(x => x.GetByDateAndLocation(It.IsAny<DateTime>().Date, existingLocation)).Returns(new List<Booking>());

            NameValueCollection appSettings = new NameValueCollection();
            appSettings.Add("fromEmail", "anyemail@email.com");

            LocationsService service = new LocationsService(logger.Object, mockLocationRepository.Object, mockBookingRepository.Object, mockDirectoryRepository.Object, mockEmailHelper.Object);
            service.ToggleLocationActive(existingLocation, false, appSettings);

            mockLocationRepository.Verify(m => m.UpdateLocation(existingLocation), Times.Once);
            mockBookingRepository.Verify(m => m.Delete(It.IsAny<List<Booking>>()), Times.Never);
            mockEmailHelper.Verify(m => m.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        [TestMethod]
        public void ToggleLocationActive_Should_Update_When_False_And_HasExistingBooking()
        {
            List<Booking> existingBookings = new List<Booking>()
            {
                new Booking(){ ID = 1, PID = "1234", Owner = "Reece" }
            };

            Location existingLocation = new Location()
            {
                ID = 1,
                Name = "Location1",
                Active = true,
                AdditionalInformation = "Original additional info"
            };

            mockBookingRepository.Setup(x => x.GetByDateAndLocation(It.IsAny<DateTime>().Date, existingLocation)).Returns(existingBookings);

            string fakeToEmailAddress = "sometoemail@email.com";
            mockDirectoryRepository.Setup(m => m.GetUser(It.IsAny<Pid>())).Returns(new User() { EmailAddress = fakeToEmailAddress });

            string fakeFromEmailAddress = "vorbs@email.com";
            string fakeBody = "Some Body Contents";
            mockEmailHelper.Setup(m => m.GetEmailMarkup(It.IsAny<string>(), It.IsAny<List<Booking>>())).Returns(fakeBody);
            mockEmailHelper.Setup(m => m.SendEmail(fakeFromEmailAddress, fakeToEmailAddress, It.IsAny<string>(), fakeBody, It.IsAny<bool>()));

            NameValueCollection appSettings = new NameValueCollection();
            appSettings.Add("fromEmail", fakeFromEmailAddress);

            LocationsService service = new LocationsService(logger.Object, mockLocationRepository.Object, mockBookingRepository.Object, mockDirectoryRepository.Object, mockEmailHelper.Object);
            service.ToggleLocationActive(existingLocation, false, appSettings);

            mockLocationRepository.Verify(m => m.UpdateLocation(existingLocation), Times.Once);
            mockBookingRepository.Verify(m => m.Delete(existingBookings), Times.Once);
            mockEmailHelper.Verify(m => m.SendEmail(fakeFromEmailAddress, fakeToEmailAddress, It.IsAny<string>(), fakeBody, It.IsAny<bool>()), Times.Once);
        }

        [TestMethod]
        public void ToggleLocationActive_Should_Update_When_False_And_HasExistingBookings()
        {
            List<Booking> existingBookings = new List<Booking>()
            {
                new Booking(){ ID = 1, PID = "1234", Owner = "Reece" },
                new Booking(){ ID = 2, PID = "5678", Owner = "Charlie" }
            };

            Location existingLocation = new Location()
            {
                ID = 1,
                Name = "Location1",
                Active = true,
                AdditionalInformation = "Original additional info"
            };

            mockBookingRepository.Setup(x => x.GetByDateAndLocation(It.IsAny<DateTime>().Date, existingLocation)).Returns(existingBookings);

            mockDirectoryRepository.Setup(m => m.GetUser(It.Is<Pid>(x => x.Identity == "1234"))).Returns(new User() { EmailAddress = "Reece@email.com" });
            mockDirectoryRepository.Setup(m => m.GetUser(It.Is<Pid>(x => x.Identity == "5678"))).Returns(new User() { EmailAddress = "Charlie@email.com" });

            string fakeFromEmailAddress = "vorbs@email.com";
            string fakeBody = "Some Body Contents";
            mockEmailHelper.Setup(m => m.GetEmailMarkup(It.IsAny<string>(), It.IsAny<List<Booking>>())).Returns(fakeBody);
            mockEmailHelper.Setup(m => m.SendEmail(fakeFromEmailAddress, It.IsAny<string>(), It.IsAny<string>(), fakeBody, It.IsAny<bool>()));

            NameValueCollection appSettings = new NameValueCollection();
            appSettings.Add("fromEmail", fakeFromEmailAddress);

            LocationsService service = new LocationsService(logger.Object, mockLocationRepository.Object, mockBookingRepository.Object, mockDirectoryRepository.Object, mockEmailHelper.Object);
            service.ToggleLocationActive(existingLocation, false, appSettings);

            mockLocationRepository.Verify(m => m.UpdateLocation(existingLocation), Times.Once);
            mockBookingRepository.Verify(m => m.Delete(It.IsAny<List<Booking>>()), Times.Once);
            mockEmailHelper.Verify(m => m.SendEmail(fakeFromEmailAddress, It.IsAny<string>(), It.IsAny<string>(), fakeBody, It.IsAny<bool>()), Times.Exactly(existingBookings.Count));
        }
    }
}
