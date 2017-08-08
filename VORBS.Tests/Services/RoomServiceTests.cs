using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using VORBS.DAL.Repositories;
using VORBS.Models;
using VORBS.Services;
using VORBS.Utils;
using VORBS.Utils.interfaces;
using static VORBS.Models.User;
using static VORBS.Services.RoomService;

namespace VORBS.Tests.Services
{
    [TestClass]
    public class RoomServiceTests
    {
        Mock<NLog.Logger> logger = new Mock<NLog.Logger>();
        Mock<ILocationRepository> mockLocationRepository = new Mock<ILocationRepository>();
        Mock<IRoomRepository> mockRoomRepository = new Mock<IRoomRepository>();
        Mock<IDirectoryService> mockDirectoryRepository = new Mock<IDirectoryService>();
        Mock<IBookingRepository> mockBookingRepository = new Mock<IBookingRepository>();

        Mock<EmailHelper> mockEmailHelper;
    
        [TestInitialize]
        public void InitTest()
        {
            Mock<ISmtpClient> smtpMock = new Mock<ISmtpClient>();
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>();
            mockEmailHelper = new Mock<EmailHelper>(MockBehavior.Strict, new object[] { smtpMock.Object, contextMock.Object });
        }



        [TestMethod]
        public void EditRoom_Should_ThrowException_If_RoomExist()
        {
            Room existingRoom = new Room()
            {
                Active = true,
                ID = 1,
                RoomName = "Room1",
                LocationID = 1,
                ComputerCount = 0,
                Location = new Location()
                {
                    ID = 1,
                    Active = true,
                    Name = "Location1"
                }
            };

            Room edittedRoom = new Room()
            {
                Active = true,
                ID = 1,
                RoomName = "Room2",
                LocationID = 1,
                ComputerCount = 3,
                Location = new Location()
                {
                    ID = 1,
                    Active = true,
                    Name = "Location1"
                }
            };

            //Mock the call where the check for an existing room is true, thus imitating a room name conflict
            mockRoomRepository.Setup(x => x.GetByLocationAndName(existingRoom.Location, edittedRoom.RoomName)).Returns(edittedRoom);
            mockRoomRepository.Setup(x => x.EditRoom(edittedRoom));

            mockLocationRepository.Setup(x => x.GetLocationById(existingRoom.LocationID)).Returns(existingRoom.Location);

            RoomService service = new RoomService(logger.Object, mockLocationRepository.Object, mockRoomRepository.Object, mockBookingRepository.Object, mockDirectoryRepository.Object, mockEmailHelper.Object);

            Assert.ThrowsException<RoomExistsException>(() => service.EditRoom(existingRoom, edittedRoom));

            mockRoomRepository.Verify(m => m.EditRoom(It.IsAny<Room>()), Times.Never);
        }

        [TestMethod]
        public void EditRoom_Should_Edit()
        {
            Room existingRoom = new Room()
            {
                Active = true,
                ID = 1,
                RoomName = "Room1",
                LocationID = 1,
                ComputerCount = 0,
                Location = new Location()
                {
                    ID = 1,
                    Active = true,
                    Name = "Location1"
                }
            };

            Room edittedRoom = new Room()
            {
                Active = true,
                ID = 1,
                RoomName = "Room1",
                LocationID = 1,
                ComputerCount = 3,
                Location = new Location()
                {
                    ID = 1,
                    Active = true,
                    Name = "Location1"
                }
            };

            mockRoomRepository.Setup(x => x.EditRoom(edittedRoom));
            mockLocationRepository.Setup(x => x.GetLocationById(existingRoom.LocationID)).Returns(existingRoom.Location);

            RoomService service = new RoomService(logger.Object, mockLocationRepository.Object, mockRoomRepository.Object, mockBookingRepository.Object, mockDirectoryRepository.Object, mockEmailHelper.Object);
            service.EditRoom(existingRoom, edittedRoom);

            mockRoomRepository.Verify(m => m.EditRoom(edittedRoom), Times.Once);
        }

        [TestMethod]
        public void ToggleRoomActive_Should_Update_When_True()
        {
            Room existingRoom = new Room()
            {
                Active = false,
                ID = 1,
                RoomName = "Room1",
            };

            NameValueCollection appSettings = new NameValueCollection();
            appSettings.Add("fromEmail", "anyemail@email.com");

            RoomService service = new RoomService(logger.Object, mockLocationRepository.Object, mockRoomRepository.Object, mockBookingRepository.Object, mockDirectoryRepository.Object, mockEmailHelper.Object);
            service.ToggleRoomActive(existingRoom, true, appSettings);

            mockRoomRepository.Verify(m => m.EditRoom(existingRoom), Times.Once);
        }

        [TestMethod]
        public void ToggleRoomActive_Should_Update_When_False_And_NoExistingBookings()
        {
            Room existingRoom = new Room()
            {
                Active = true,
                ID = 1,
                RoomName = "Room1",
            };

            mockBookingRepository.Setup(m => m.GetByDateAndRoom(It.IsAny<DateTime>(), existingRoom)).Returns(new List<Booking>());

            NameValueCollection appSettings = new NameValueCollection();
            appSettings.Add("fromEmail", "anyemail@email.com");

            RoomService service = new RoomService(logger.Object, mockLocationRepository.Object, mockRoomRepository.Object, mockBookingRepository.Object, mockDirectoryRepository.Object, mockEmailHelper.Object);
            service.ToggleRoomActive(existingRoom, false, appSettings);

            mockRoomRepository.Verify(m => m.EditRoom(existingRoom), Times.Once);
            mockBookingRepository.Verify(m => m.Delete(It.IsAny<List<Booking>>()), Times.Never);
        }

        [TestMethod]
        public void ToggleRoomActive_Should_Update_When_False_And_HasExistingBookings()
        {
            List<Booking> existingBookings = new List<Booking>()
            {
                new Booking(){ ID = 1, PID = "1234", Owner = "Reece" }
            };

            Room existingRoom = new Room()
            {
                Active = true,
                ID = 1,
                RoomName = "Room1",
            };

            mockBookingRepository.Setup(m => m.GetByDateAndRoom(It.IsAny<DateTime>(), existingRoom)).Returns(existingBookings);

            string fakeToEmailAddress = "sometoemail@email.com";
            mockDirectoryRepository.Setup(m => m.GetUser(It.IsAny<Pid>())).Returns(new User() { EmailAddress = fakeToEmailAddress });

            string fakeFromEmailAddress = "vorbs@email.com";
            string fakeBody = "Some Body Contents";
            mockEmailHelper.Setup(m => m.GetEmailMarkup(It.IsAny<string>(), It.IsAny<List<Booking>>())).Returns(fakeBody);
            mockEmailHelper.Setup(m => m.SendEmail(fakeFromEmailAddress, fakeToEmailAddress, It.IsAny<string>(), fakeBody, It.IsAny<bool>()));

            NameValueCollection appSettings = new NameValueCollection();
            appSettings.Add("fromEmail", fakeFromEmailAddress);

            RoomService service = new RoomService(logger.Object, mockLocationRepository.Object, mockRoomRepository.Object, mockBookingRepository.Object, mockDirectoryRepository.Object, mockEmailHelper.Object);
            service.ToggleRoomActive(existingRoom, false, appSettings);

            mockRoomRepository.Verify(m => m.EditRoom(existingRoom), Times.Once);
            mockBookingRepository.Verify(m => m.Delete(It.IsAny<List<Booking>>()), Times.Once);
            mockEmailHelper.Verify(m => m.SendEmail(fakeFromEmailAddress, fakeToEmailAddress, It.IsAny<string>(), fakeBody, It.IsAny<bool>()), Times.Once);
        }
    }
}
