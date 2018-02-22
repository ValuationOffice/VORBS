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

            _logger.Trace(LoggerHelper.InitializeClassMessage());
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
            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(usersDTO));
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
                {
                    _logger.Debug($"Name is null or empty");
                    return userDTO;
                }
                    
                        
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
            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(userDTO, name));
            return userDTO;
        }

        [Route("{payID}/{name:bool}")]
        [HttpGet]
        public string GetFullNameByPID(string pid, bool name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pid))
                {
                    _logger.Debug($"PID is null or empty");
                    return null;
                }


                if (name)
                {
                    string user = _directoryService.GetUser(new User.Pid(pid)).FullName;
                    _logger.Trace(LoggerHelper.ExecutedFunctionMessage(user, pid, name));
                    return user;
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to get user by PID: " + pid, ex);
            }
            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(null, pid, name));
            return null;
        }
    }
}
