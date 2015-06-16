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
        private NLog.Logger _logger;
        private VORBSContext db = new VORBSContext();

        public UsersController()
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
        }

        [Route("{allUsers:bool}")]
        [HttpGet]
        public List<AdminDTO> GetAllUsers()
        {
            List<AdminDTO> usersDTO = new List<AdminDTO>();
            try
            {
                usersDTO = AdQueries.AllUserDetails();
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get list of users from AD", ex);
            }
            return usersDTO;
        }


        [Route("{name}")]
        [HttpGet]
        public List<AdminDTO> GetAvailableUsers(string name)
        {
            List<AdminDTO> userDTO = new List<AdminDTO>();
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return new List<AdminDTO>();

                userDTO = AdQueries.FindUserDetails(name);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get list of users from AD by name: " + name, ex);
            }
            return userDTO;
        }

        [Route("{payID}/{name:bool}")]
        [HttpGet]
        public string GetFullNameByPID(string pid, bool name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pid))
                    return null;

                if (name)
                    return AdQueries.GetUserByPid(pid).Name;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get user by PID: " + pid, ex);
            }
            return null;
        }
    }
}
