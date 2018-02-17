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
using VORBS.Services;

namespace VORBS.API
{
    [RoutePrefix("api/users")]
    public class UsersController : ApiController
    {
        private NLog.Logger _logger;
        private IDirectoryService _directoryService;

        public UsersController(IDirectoryService directoryService)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();

            _directoryService = directoryService;
        }

        [HttpGet]
        public List<UserDTO> GetAllUsers()
        {
            List<UserDTO> usersDTO = new List<UserDTO>();
            try
            {
                List<User> usersResult = _directoryService.GetAllUsers();
                foreach (User user in usersResult)
                {
                    usersDTO.Add(new UserDTO()
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.EmailAddress,
                        PID = user.PayId.Identity
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get list of users from AD", ex);
            }
            return usersDTO;
        }


        [Route("{name}")]
        [HttpGet]
        public List<UserDTO> GetAvailableUsers(string name)
        {
            List<UserDTO> userDTO = new List<UserDTO>();
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return userDTO;
                        
                List<User> userResults = _directoryService.GetBySurname(name);
                foreach (User user in userResults)
                {
                    userDTO.Add(new UserDTO()
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.EmailAddress,
                        PID = user.PayId.Identity
                    });
                }
                
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
                    return _directoryService.GetUser(new User.Pid(pid)).FullName;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get user by PID: " + pid, ex);
            }
            return null;
        }
    }
}
