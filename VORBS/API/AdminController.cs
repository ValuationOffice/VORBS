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

namespace VORBS.API
{
    [RoutePrefix("api/admin")]
    [VORBS.Security.VorbsApiAuthoriseAttribute(2)]
    public class AdminController : ApiController
    {
        private NLog.Logger _logger;
        private AdminRepository _adminRepository;
        private BookingRepository _bookingRepository;
        private VORBSContext db;

        public AdminController(VORBSContext context)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
            db = context;
            _adminRepository = new AdminRepository(db);
            _bookingRepository = new BookingRepository(db);
        }

        public AdminController() : this(new VORBSContext()) { }


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
                if (_adminRepository.GetAdminByPid(admin.PID) != null)
                    return Request.CreateResponse(HttpStatusCode.Conflict);

                _adminRepository.SaveNewAdmin(admin);

                _logger.Info("Admin successfully added: " + admin.PID);

                return new HttpResponseMessage(HttpStatusCode.OK);
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
                //Find Existing Booking
                Admin existingAdmin = _adminRepository.GetAdminById(existingAdminId);

                //No edits found
                if (existingAdmin.PermissionLevel == editAdmin.PermissionLevel && existingAdmin.LocationID == editAdmin.LocationID)
                    return new HttpResponseMessage(HttpStatusCode.NotModified);

                existingAdmin.PermissionLevel = editAdmin.PermissionLevel;
                existingAdmin.LocationID = editAdmin.LocationID;

                _adminRepository.UpdateAdmin(existingAdmin);

                _logger.Info("Admin successfully Edited: " + editAdmin.PID);

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
                _adminRepository.DeleteAdmin(admin);

                _logger.Info("Admin successfully deleted: " + adminId);

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
