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
        public List<UserDTO> GetAllUsers() 
        {
            List<UserDTO> usersDTO = new List<UserDTO>();
            usersDTO = AdQueries.AllUserDetails();
            return usersDTO;
        }


        [Route("{surname}")]
        [HttpGet]
        public List<UserDTO> GetAvailableUsers(string surname)
         {            
            List<UserDTO> userDTO = new List<UserDTO>();
            if (string.IsNullOrWhiteSpace(surname))
                return new List<UserDTO>();

            userDTO = AdQueries.FindUserDetails(surname);
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
