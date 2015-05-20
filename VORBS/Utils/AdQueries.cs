using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
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

        public static List<UserDTO> AllUserDetails()
        {
            PrincipalContext context = new PrincipalContext(ContextType.Domain);
            UserPrincipal userPrincipal = new UserPrincipal(context);
            PrincipalSearcher search = new PrincipalSearcher(userPrincipal);
            List<UserDTO> users = new List<UserDTO>();
            foreach (UserPrincipal result in search.FindAll())
            {
                if (!string.IsNullOrWhiteSpace(result.EmailAddress))
                    users.Add(new UserDTO() { EmailAddress = result.EmailAddress, Name = result.Name });
            }

            return users;
        }

        public static List<UserDTO> FindUserDetails(string surname)
        {
            PrincipalContext sContext = new PrincipalContext(ContextType.Domain);
            UserPrincipal userPrincipal = new UserPrincipal(sContext);
            userPrincipal.Surname = surname + "*";
            PrincipalSearcher search = new PrincipalSearcher(userPrincipal);            
            List<UserDTO> user = new List<UserDTO>();
            foreach (UserPrincipal result in search.FindAll())
            {
                if (!string.IsNullOrWhiteSpace(result.EmailAddress))
                    user.Add(new UserDTO() { EmailAddress = result.EmailAddress, Name = result.Name });
            }

            return user;
        }
    }
}