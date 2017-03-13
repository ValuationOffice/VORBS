using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Web;
using VORBS.Models;
using VORBS.Models.DTOs;

namespace VORBS.Services
{
    public class ActiveDirectoryService : IDirectoryService
    {
        PrincipalContext context = null;

        public ActiveDirectoryService()
        {
            context = new PrincipalContext(ContextType.Domain);
        }

        public List<User> GetAllUsers()
        {
            PrincipalContext context = new PrincipalContext(ContextType.Domain);
            UserPrincipal userPrincipal = new UserPrincipal(context);
            PrincipalSearcher search = new PrincipalSearcher(userPrincipal);
            List<User> users = new List<User>();
            foreach (UserPrincipal result in search.FindAll())
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

            return users;
        }

        public List<User> GetBySurname(string surname)
        {
            PrincipalContext sContext = new PrincipalContext(ContextType.Domain);
            UserPrincipal userPrincipal = new UserPrincipal(sContext);
            userPrincipal.Surname = surname + "*";
            PrincipalSearcher search = new PrincipalSearcher(userPrincipal);
            List<User> users = new List<User>();
            foreach (UserPrincipal result in search.FindAll())
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

            return users;
        }

        public User GetCurrentUser(string identity)
        {
            PrincipalContext sContext = new PrincipalContext(ContextType.Domain);
            if (identity == null)
                return null;

            //Remove the domain string
            string pid = identity.Substring(identity.IndexOf("\\") + 1);

            UserPrincipal result = UserPrincipal.FindByIdentity(sContext, IdentityType.SamAccountName, pid);

            if (!string.IsNullOrWhiteSpace(result.SamAccountName) && !string.IsNullOrWhiteSpace(result.EmailAddress) &&
                   (!string.IsNullOrEmpty(result.GivenName) || !string.IsNullOrEmpty(result.Surname)))
            {
                return new User()
                {
                    FirstName = result.GivenName,
                    LastName = result.Surname,
                    PayId = new User.Pid(result.SamAccountName),
                    EmailAddress = result.EmailAddress
                };
            }
            else
                return null;

        }

        public User GetUser(User.Pid pid)
        {
            PrincipalContext sContext = new PrincipalContext(ContextType.Domain);
            UserPrincipal result = UserPrincipal.FindByIdentity(sContext, IdentityType.SamAccountName, pid.Identity);

            if (!string.IsNullOrWhiteSpace(result.SamAccountName) && !string.IsNullOrWhiteSpace(result.EmailAddress) &&
                    (!string.IsNullOrEmpty(result.GivenName) || !string.IsNullOrEmpty(result.Surname)))
            {
                return new User()
                {
                    FirstName = result.GivenName,
                    LastName = result.Surname,
                    PayId = new User.Pid(result.SamAccountName),
                    EmailAddress = result.EmailAddress
                };
            }
            else
                return null;
        }
    }
}