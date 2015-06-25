using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VORBS.DAL;
using VORBS.Models;
using VORBS.Models.DTOs;

namespace VORBS.API
{
    [RoutePrefix("api/admin")]
    public class AdminController : ApiController
    {
        private NLog.Logger _logger;
        private VORBSContext db = new VORBSContext();

        public AdminController()
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
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

                List<Admin> admins = db.Admins.ToList();

                admins.ForEach(a => adminsDTO.Add(new AdminDTO()
                {
                    ID = a.ID,
                    PID = a.PID,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    Email = a.Email,
                    Location = a.Location,
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
                Admin admin = db.Admins.Single(a => a.ID == adminId);

                return new AdminDTO()
                {
                    ID = admin.ID,
                    PID = admin.PID,
                    FirstName = admin.FirstName,
                    LastName = admin.LastName,
                    Email = admin.Email,
                    Location = admin.Location,
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
                if (db.Admins.Count(a => a.PID == admin.PID) > 0)
                    return Request.CreateResponse(HttpStatusCode.Conflict);

                db.Admins.Add(admin);
                db.SaveChanges();

                _logger.Info("Admin sucessfully added: " + admin.PID);

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
                Admin existingAdmin = db.Admins.Single(a => a.ID == existingAdminId);

                //No edits found
                if (existingAdmin.PermissionLevel == editAdmin.PermissionLevel && existingAdmin.Location == editAdmin.Location)
                    return new HttpResponseMessage(HttpStatusCode.NotModified);

                existingAdmin.PermissionLevel = editAdmin.PermissionLevel;
                existingAdmin.Location = editAdmin.Location;

                db.SaveChanges();

                _logger.Info("Admin sucessfully Edited: " + editAdmin.PID);

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
                db.Admins.Remove(db.Admins.Single(a => a.ID == adminId));
                db.SaveChanges();

                _logger.Info("Admin sucessfully deleted: " + adminId);

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
                return db.Bookings.Select(b => b.Owner).Distinct().ToList();
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get list of booking owners", ex);
                return new List<string>();
            }
        }
    }
}
