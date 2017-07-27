using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VORBS.DAL.Repositories;
using VORBS.Models;
using VORBS.Services;
using static VORBS.Services.AdminService;

namespace VORBS.Tests.Services
{
    [TestClass]
    public class AdminServiceTests
    {
        Admin exampleAdmin = new Admin()
        {
            PID = "1234",
            ID = 1,
            Email = "email@email.com",
            FirstName = "fakeFirstName",
            LastName = "fakeLastName",
            LocationID = 1,
            PermissionLevel = 2
        };

        Mock<NLog.Logger> logger = new Mock<NLog.Logger>();

        [TestMethod]
        public void AddNewAdmin_Should_AddAdmin_If_DoesNotExist()
        {
            var mockRepo = new Mock<IAdminRepository>();
            mockRepo.Setup(x => x.GetAdminByPid(exampleAdmin.PID))
                .Returns((Admin)null);

            AdminService service = new AdminService(logger.Object, mockRepo.Object);

            service.AddNewAdmin(exampleAdmin);
            mockRepo.Verify(m => m.SaveNewAdmin(exampleAdmin), Times.Once);
        }

        [TestMethod]
        public void AddNewAdmin_Should_ThrowException_When_Exists()
        {
            var mockRepo = new Mock<IAdminRepository>();
            mockRepo.Setup(x => x.GetAdminByPid(exampleAdmin.PID))
                .Returns(exampleAdmin);

            AdminService service = new AdminService(logger.Object, mockRepo.Object);

            Assert.ThrowsException<AdminExistsException>(() => service.AddNewAdmin(exampleAdmin));
            
            mockRepo.Verify(m => m.SaveNewAdmin(exampleAdmin), Times.Never);
        }

        [TestMethod]
        public void DeleteExistingAdmin_Should_DeleteAdmin()
        {
            var mockRepo = new Mock<IAdminRepository>();
            mockRepo.Setup(x => x.DeleteAdmin(exampleAdmin));

            AdminService service = new AdminService(logger.Object, mockRepo.Object);

            service.DeleteExistingAdmin(exampleAdmin);

            mockRepo.Verify(m => m.DeleteAdmin(exampleAdmin), Times.Once);
        }

        [TestMethod]
        public void EditExistingAdmin_Should_Update()
        {
            Admin updateAdmin = new Admin()
            {
                Email = exampleAdmin.Email,
                FirstName = exampleAdmin.FirstName,
                LastName = exampleAdmin.LastName,
                ID = exampleAdmin.ID,
                PID = exampleAdmin.PID,
                PermissionLevel = 1,
                LocationID = 2
            };

            var mockRepo = new Mock<IAdminRepository>();
            mockRepo.Setup(x => x.UpdateAdmin(updateAdmin));

            AdminService service = new AdminService(logger.Object, mockRepo.Object);
            service.EditExistingAdmin(exampleAdmin, updateAdmin);

            mockRepo.Verify(m => m.UpdateAdmin(exampleAdmin), Times.Once);
        }
    }
}
