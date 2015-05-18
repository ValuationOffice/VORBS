using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Permissions;
using System.Web;
using VORBS.Models.DTOs;

namespace VORBS.Utils
{
    public static class AdQueries
    {
        public static UserPrincipal GetUserByCurrentUser(string currentIdentity)
        {
            if (currentIdentity == null)           
                return null;

            //Remove the domain string
            string pid = currentIdentity.Substring(currentIdentity.IndexOf("\\") + 1);

            PrincipalContext context = new PrincipalContext(ContextType.Domain);

            return UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, pid);
        }

        public static UserPrincipal GetUserByPid(string pid)
        {
            PrincipalContext context = new PrincipalContext(ContextType.Domain);

            return UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, pid);
        }

        public static List<UserDTO> UserDetails(string emailAddress) 
        {
            PrincipalContext context = new PrincipalContext(ContextType.Domain);
            UserPrincipal userPrincipal = new UserPrincipal(context);
            userPrincipal.EmailAddress = emailAddress + "*";
            PrincipalSearcher search = new PrincipalSearcher(userPrincipal);            
            List<UserDTO> user = new List<UserDTO>();
            foreach (UserPrincipal result in search.FindAll())
            {
                user.Add( new UserDTO() { EmailAddress = result.EmailAddress.ToString(), Name = result.DisplayName.ToString()});
            }

            return user;
        }
    }
}