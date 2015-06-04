using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VORBS.DAL;
using VORBS.Models.DTOs;

namespace VORBS.API
{
    [RoutePrefix("api/admin")]
    public class AdminController : ApiController
    {
        private VORBSContext db = new VORBSContext();

        [HttpGet]
        [Route("{allUsers}")]
        public List<AdminDTO> GetAllAdminUsers(string allUsers)
        {
            List<AdminDTO> users = new List<AdminDTO>();

            try
            {
                if (allUsers.Equals("userId"))
                {

                    
                }
            }
            catch (Exception)
            {
                return null;
            }

            return users;
        }

        [HttpGet]
        [Route("{userId:int}")]
        public AdminDTO GetAdminUserById(int userId)
        {
            try
            {
                return null;//db.
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
