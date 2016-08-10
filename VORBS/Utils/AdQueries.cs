using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Web;
using VORBS.Models.DTOs;

namespace VORBS.Utils
{
    public class AdQueries
    {
        PrincipalContext context = null;

        public AdQueries()
        {
            context = new PrincipalContext(ContextType.Domain);
        }

        public static UserPrincipal GetUserByCurrentUser(string currentIdentity)
        {
            PrincipalContext sContext = new PrincipalContext(ContextType.Domain);
            if (currentIdentity == null)
                return null;

            //Remove the domain string
            string pid = currentIdentity.Substring(currentIdentity.IndexOf("\\") + 1);

            return UserPrincipal.FindByIdentity(sContext, IdentityType.SamAccountName, pid);
        }

        public static UserPrincipal GetUserByPid(string pid)
        {
            PrincipalContext sContext = new PrincipalContext(ContextType.Domain);
            return UserPrincipal.FindByIdentity(sContext, IdentityType.SamAccountName, pid);
        }

        public string GetUserFullNameByPid(string pid)
        {
            if (string.IsNullOrWhiteSpace(pid))
                return "Unknown";

            UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, pid);

            if (user == null)
                return "Unknown";

            return user.Name;
        }

        public static List<AdminDTO> AllUserDetails()
        {
            PrincipalContext context = new PrincipalContext(ContextType.Domain);
            UserPrincipal userPrincipal = new UserPrincipal(context);
            PrincipalSearcher search = new PrincipalSearcher(userPrincipal);
            List<AdminDTO> users = new List<AdminDTO>();
            foreach (UserPrincipal result in search.FindAll())
            {
                if (!string.IsNullOrWhiteSpace(result.SamAccountName) && !string.IsNullOrWhiteSpace(result.EmailAddress) &&
                   (!string.IsNullOrEmpty(result.GivenName) || !string.IsNullOrEmpty(result.Surname)))

                    users.Add(new AdminDTO()
                    {
                        FirstName = result.GivenName,
                        LastName = result.Surname,
                        PID = result.SamAccountName,
                        Email = result.EmailAddress
                    });
            }

            return users;
        }

        public static List<AdminDTO> FindUserDetails(string name)
        {
            PrincipalContext sContext = new PrincipalContext(ContextType.Domain);
            UserPrincipal userPrincipal = new UserPrincipal(sContext);
            userPrincipal.Surname = name + "*";
            PrincipalSearcher search = new PrincipalSearcher(userPrincipal);
            List<AdminDTO> users = new List<AdminDTO>();
            foreach (UserPrincipal result in search.FindAll())
            {
                if (!string.IsNullOrWhiteSpace(result.SamAccountName) && !string.IsNullOrWhiteSpace(result.EmailAddress) &&
                   (!string.IsNullOrEmpty(result.GivenName) || !string.IsNullOrEmpty(result.Surname)))

                    users.Add(new AdminDTO() 
                    { 
                        FirstName = result.GivenName,
                        LastName = result.Surname,
                        PID = result.SamAccountName,
                        Email = result.EmailAddress
                    });
            }

            return users;
        }

        public static bool IsOffline()
        {
            string offline = ConfigurationManager.AppSettings["offline-mode"];
            if (!string.IsNullOrEmpty(offline))
            {
                bool isOff = bool.Parse(offline);
                return isOff;
            }
            return false;
            //Added the logic above to manage offline-state using config because depending on env variables for what ever reason is not 100% working in live. Live servers for you...
            // return (Environment.UserDomainName != "VOAITDEV" && Environment.UserDomainName != "VOA_GPN_GOV_UK") ? true : false;
        }

        public static UserPrincipal CreateFakeUser()
        {
            PrincipalContext sContext = new PrincipalContext(ContextType.Machine);

            return  new UserPrincipal(sContext)
            {
                SamAccountName = "localuser",
                Enabled = true,
                Name = "localuser"
            };
        }
    }
}