using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using VORBS.DAL.Repositories;
using VORBS.Services;
using VORBS.Utils;
using VORBS.Utils.interfaces;

namespace VORBS.Tests.Services
{
    public abstract class BaseServiceTestSetup
    {
        protected Mock<NLog.Logger> logger;
        protected Mock<ILocationRepository> mockLocationRepository;
        protected Mock<IAdminRepository> mockAdminRepository;
        protected Mock<IDirectoryService> mockDirectoryRepository;
        protected Mock<IRoomRepository> mockRoomRepository;
        protected Mock<IBookingRepository> mockBookingRepository;

        protected Mock<EmailHelper> mockEmailHelper;

        [SetUp]
        public void InitTest()
        {
            logger = new Mock<NLog.Logger>();
            mockLocationRepository = new Mock<ILocationRepository>();
            mockAdminRepository = new Mock<IAdminRepository>();
            mockDirectoryRepository = new Mock<IDirectoryService>();
            mockRoomRepository = new Mock<IRoomRepository>();
            mockBookingRepository = new Mock<IBookingRepository>();

            Mock<ISmtpClient> smtpMock = new Mock<ISmtpClient>();
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>();
            mockEmailHelper = new Mock<EmailHelper>(MockBehavior.Strict, new object[] { smtpMock.Object, contextMock.Object });
        }
    }
}
