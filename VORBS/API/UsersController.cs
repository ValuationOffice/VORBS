using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VORBS.Models;
using VORBS.DAL;
using VORBS.Models.DTOs;
using VORBS.Utils;

namespace VORBS.API
{
    [RoutePrefix("api/users")]
    public class UsersController : ApiController
    {
        private VORBSContext db = new VORBSContext();


        [Route("{allUsers:bool}")]
        [HttpGet]
        public List<AdminDTO> GetAllUsers() 
        {
            List<AdminDTO> usersDTO = new List<AdminDTO>();
            usersDTO = AdQueries.AllUserDetails();
            return usersDTO;
        }


        [Route("{name}")]
        [HttpGet]
        public List<AdminDTO> GetAvailableUsers(string name)
         {
             List<AdminDTO> userDTO = new List<AdminDTO>();
            if (string.IsNullOrWhiteSpace(name))
                return new List<AdminDTO>();

            userDTO = AdQueries.FindUserDetails(name);
            return userDTO;
        }

        [Route("{payID}/{name:bool}")]
        [HttpGet]
        public string GetFullNameByPID(string pid, bool name) 
        {            
            if (string.IsNullOrWhiteSpace(pid))
                return null;

            if (name)
                return AdQueries.GetUserByPid(pid).Name;

            return null;
        }
    }
}
