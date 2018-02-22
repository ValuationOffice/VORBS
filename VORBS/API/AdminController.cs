using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VORBS.DAL;
using VORBS.DAL.Repositories;
using VORBS.Models;
using VORBS.Models.DTOs;
using VORBS.Services;
using VORBS.Utils;
using static VORBS.Services.AdminService;

namespace VORBS.API
{
    [RoutePrefix("api/admin")]
    [VORBS.Security.VorbsApiAuthoriseAttribute(2)]
    public class AdminController : ApiController
    {
        private NLog.Logger _logger;
        private AdminService _adminService;

        private IAdminRepository _adminRepository;
        private IBookingRepository _bookingRepository;

        public AdminController(IBookingRepository bookingRepository, IAdminRepository adminRepository)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _adminService = new AdminService(_logger, adminRepository);

            _adminRepository = adminRepository;
            _bookingRepository = bookingRepository;

            _logger.Trace(LoggerHelper.InitializeClassMessage());
        }

        [HttpGet]
        [Route("{allAdmins}")]
        public List<AdminDTO> GetAllAdminUsers(string allAdmins)
        {
            try
            {
                if (!allAdmins.Equals("adminId"))
                    return new List<AdminDTO>();

                List<AdminDTO> adminsDTO = new List<AdminDTO>();

                List<Admin> admins = _adminRepository.GetAll();

                admins.ForEach(a => adminsDTO.Add(new AdminDTO()
                {
                    ID = a.ID,
                    PID = a.PID,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    Email = a.Email,
                    Location = new LocationDTO()
                    {
                        Active = a.Location.Active,
                        ID = a.Location.ID,
                        Name = a.Location.Name
                    },
                    PermissionLevel = a.PermissionLevel
                }));

                return adminsDTO;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get list of admins", ex);
                return new List<AdminDTO>();
            }
        }

        [HttpGet]
        [Route("{adminId:int}")]
        public AdminDTO GetAdminUserById(int adminId)
        {
            try
            {
                Admin admin = _adminRepository.GetAdminById(adminId);

                return new AdminDTO()
                {
                    ID = admin.ID,
                    PID = admin.PID,
                    FirstName = admin.FirstName,
                    LastName = admin.LastName,
                    Email = admin.Email,
                    Location = new LocationDTO()
                    {
                        Active = admin.Location.Active,
                        ID = admin.Location.ID,
                        Name = admin.Location.Name
                    },
                    PermissionLevel = admin.PermissionLevel
                };
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get admin", ex);
                return null;
            }
        }

        [HttpPost]
        public HttpResponseMessage AddNewAdmin(Admin admin)
        {
            try
            {
                _adminService.AddNewAdmin(admin);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (AdminExistsException e)
            {
                return Request.CreateResponse(HttpStatusCode.Conflict);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to add new admin: " + admin.PID, ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [Route("{existingAdminId:int}")]
        public HttpResponseMessage EditAdmin(int existingAdminId, Admin editAdmin)
        {
            try
            {
                Admin existingAdmin = _adminRepository.GetAdminById(existingAdminId);

                if (existingAdmin.PermissionLevel == editAdmin.PermissionLevel && existingAdmin.LocationID == editAdmin.LocationID)
                    return new HttpResponseMessage(HttpStatusCode.NotModified);

                _adminService.EditExistingAdmin(existingAdmin, editAdmin);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to edit new admin: " + editAdmin.PID, ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpDelete]
        [Route("{adminId:int}")]
        public HttpResponseMessage DeleteAdminUserById(int adminId)
        {
            try
            {
                Admin admin = _adminRepository.GetAdminById(adminId);

                _adminService.DeleteExistingAdmin(admin);

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to delete admin: " + adminId, ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public List<string> GetAllBookingOwners()
        {
            try
            {
                return _bookingRepository.GetDistinctListOfOwners();
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get list of booking owners", ex);
                return new List<string>();
            }
        }
    }
}
