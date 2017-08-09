using Moq;
using NUnit.Framework;
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
    [TestFixture]
    public class AdminServiceTests : BaseServiceTestSetup
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

        [Test]
        public void AddNewAdmin_Should_AddAdmin_If_DoesNotExist()
        {
            mockAdminRepository.Setup(x => x.GetAdminByPid(exampleAdmin.PID))
                .Returns((Admin)null);

            AdminService service = new AdminService(logger.Object, mockAdminRepository.Object);

            service.AddNewAdmin(exampleAdmin);
            mockAdminRepository.Verify(m => m.SaveNewAdmin(exampleAdmin), Times.Once);
        }

        [Test]
        public void AddNewAdmin_Should_ThrowException_When_Exists()
        {
            mockAdminRepository.Setup(x => x.GetAdminByPid(exampleAdmin.PID))
                .Returns(exampleAdmin);

            AdminService service = new AdminService(logger.Object, mockAdminRepository.Object);

            Assert.Throws<AdminExistsException>(() => service.AddNewAdmin(exampleAdmin));

            mockAdminRepository.Verify(m => m.SaveNewAdmin(exampleAdmin), Times.Never);
        }

        [Test]
        public void DeleteExistingAdmin_Should_DeleteAdmin()
        {
            mockAdminRepository.Setup(x => x.DeleteAdmin(exampleAdmin));

            AdminService service = new AdminService(logger.Object, mockAdminRepository.Object);

            service.DeleteExistingAdmin(exampleAdmin);

            mockAdminRepository.Verify(m => m.DeleteAdmin(exampleAdmin), Times.Once);
        }

        [Test]
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

            mockAdminRepository.Setup(x => x.UpdateAdmin(updateAdmin));

            AdminService service = new AdminService(logger.Object, mockAdminRepository.Object);
            service.EditExistingAdmin(exampleAdmin, updateAdmin);

            mockAdminRepository.Verify(m => m.UpdateAdmin(exampleAdmin), Times.Once);
        }
    }
}
