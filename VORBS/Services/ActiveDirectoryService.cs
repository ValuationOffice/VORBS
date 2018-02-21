using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Web;
using VORBS.Models;
using VORBS.Models.DTOs;
using VORBS.Utils;

namespace VORBS.Services
{
    public class ActiveDirectoryService : IDirectoryService
    {
        PrincipalContext context = null;
        ILogger _logger;

        public ActiveDirectoryService()
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();

            context = new PrincipalContext(ContextType.Domain);
            _logger.Trace(LoggerHelper.InitializeClassMessage($"Context: {context.Name}", $"Server: {context.ConnectedServer}"));
        }

        public List<User> GetAllUsers()
        {
            PrincipalContext context = new PrincipalContext(ContextType.Domain);
            _logger.Debug($"Using PrincipalContext: {context.Name} - Server: {context.ConnectedServer}");
            UserPrincipal userPrincipal = new UserPrincipal(context);
            _logger.Debug($"Using UserPrincipal: {userPrincipal.Name} - Default");

            PrincipalSearcher search = new PrincipalSearcher(userPrincipal);
            List<User> users = new List<User>();
            PrincipalSearchResult<Principal> searchResults = search.FindAll();

            _logger.Debug($"Principal search executed. Result: Count: {searchResults.Count()}");

            foreach (UserPrincipal result in searchResults)
            {
                if (!string.IsNullOrWhiteSpace(result.SamAccountName) && !string.IsNullOrWhiteSpace(result.EmailAddress) &&
                   (!string.IsNullOrEmpty(result.GivenName) || !string.IsNullOrEmpty(result.Surname)))

                    users.Add(new User()
                    {
                        FirstName = result.GivenName,
                        LastName = result.Surname,
                        PayId = new User.Pid(result.SamAccountName),
                        EmailAddress = result.EmailAddress
                    });
            }

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(users));

            return users;
        }

        public List<User> GetBySurname(string surname)
        {
            PrincipalContext sContext = new PrincipalContext(ContextType.Domain);
            _logger.Debug($"Using PrincipalContext: {context.Name} - Server: {context.ConnectedServer}");
            UserPrincipal userPrincipal = new UserPrincipal(sContext);
            _logger.Debug($"Using UserPrincipal: {userPrincipal.Name} - Default");

            userPrincipal.Surname = surname + "*";
            PrincipalSearcher search = new PrincipalSearcher(userPrincipal);
            List<User> users = new List<User>();

            PrincipalSearchResult<Principal> searchResults = search.FindAll();
            _logger.Debug($"Principal search executed. Result: Count: {searchResults.Count()}");

            foreach (UserPrincipal result in searchResults)
            {
                if (!string.IsNullOrWhiteSpace(result.SamAccountName) && !string.IsNullOrWhiteSpace(result.EmailAddress) &&
                   (!string.IsNullOrEmpty(result.GivenName) || !string.IsNullOrEmpty(result.Surname)))

                    users.Add(new User()
                    {
                        FirstName = result.GivenName,
                        LastName = result.Surname,
                        PayId = new User.Pid(result.SamAccountName),
                        EmailAddress = result.EmailAddress
                    });
            }

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(users, surname));

            return users;
        }

        public User GetCurrentUser(string identity)
        {
            PrincipalContext sContext = new PrincipalContext(ContextType.Domain);
            _logger.Debug($"Using PrincipalContext: {context.Name} - Server: {context.ConnectedServer}");

            if (identity == null)
            {
                _logger.Error($"Identity is null");
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(null, identity));
                return null;
            }

            //Remove the domain string
            string pid = identity.Substring(identity.IndexOf("\\") + 1);

            _logger.Debug($"Searching by identity of: Pid-{pid}");
            UserPrincipal result = UserPrincipal.FindByIdentity(sContext, IdentityType.SamAccountName, pid);

            if (!string.IsNullOrWhiteSpace(result.SamAccountName) && !string.IsNullOrWhiteSpace(result.EmailAddress) &&
                   (!string.IsNullOrEmpty(result.GivenName) || !string.IsNullOrEmpty(result.Surname)))
            {
                User currentUser = new User()
                {
                    FirstName = result.GivenName,
                    LastName = result.Surname,
                    PayId = new User.Pid(result.SamAccountName),
                    EmailAddress = result.EmailAddress
                };

                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(currentUser, identity));

                return currentUser;
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder();

                if (!string.IsNullOrWhiteSpace(result.SamAccountName))
                    stringBuilder.Append($"SamAccountName value: {result.SamAccountName}, ");
                if (!string.IsNullOrWhiteSpace(result.EmailAddress))
                    stringBuilder.Append($"EmailAddress value: {result.EmailAddress}, ");
                if (!string.IsNullOrEmpty(result.GivenName))
                    stringBuilder.Append($"GivenName value: {result.GivenName}, ");
                if (!string.IsNullOrEmpty(result.Surname))
                    stringBuilder.Append($"Surname value: {result.Surname} ");

                _logger.Error($"Value is missing: {stringBuilder.ToString()}");

                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(null, identity));
                return null;
            }

        }

        public User GetUser(User.Pid pid)
        {
            PrincipalContext sContext = new PrincipalContext(ContextType.Domain);
            _logger.Debug($"Using PrincipalContext: {context.Name} - Server: {context.ConnectedServer}");

            _logger.Debug($"Searching by identity of: Pid-{pid}");
            UserPrincipal result = UserPrincipal.FindByIdentity(sContext, IdentityType.SamAccountName, pid.Identity);

            if (!string.IsNullOrWhiteSpace(result.SamAccountName) && !string.IsNullOrWhiteSpace(result.EmailAddress) &&
                    (!string.IsNullOrEmpty(result.GivenName) || !string.IsNullOrEmpty(result.Surname)))
            {
                User user = new User()
                {
                    FirstName = result.GivenName,
                    LastName = result.Surname,
                    PayId = new User.Pid(result.SamAccountName),
                    EmailAddress = result.EmailAddress
                };

                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(user, pid));

                return user;
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder();

                if (!string.IsNullOrWhiteSpace(result.SamAccountName))
                    stringBuilder.Append($"SamAccountName value: {result.SamAccountName}, ");
                if (!string.IsNullOrWhiteSpace(result.EmailAddress))
                    stringBuilder.Append($"EmailAddress value: {result.EmailAddress}, ");
                if (!string.IsNullOrEmpty(result.GivenName))
                    stringBuilder.Append($"GivenName value: {result.GivenName}, ");
                if (!string.IsNullOrEmpty(result.Surname))
                    stringBuilder.Append($"Surname value: {result.Surname} ");

                _logger.Error($"Value is missing: {stringBuilder.ToString()}");

                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(null, pid));
                return null;
            }   
        }
    }
}