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
        private VORBSContext db = new VORBSContext();

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
                //TODO: Log Exception
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
                //TODO: Log Exception
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

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                //TODO: Log Exception
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

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                //TODO: Log Exception
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
            catch (Exception)
            {
                return new List<string>();
            }
        }
    }
}
