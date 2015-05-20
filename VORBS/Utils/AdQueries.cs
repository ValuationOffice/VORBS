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

        public UserPrincipal GetUserByCurrentUser(string currentIdentity)
        {
            if (currentIdentity == null)           
                return null;

            //Remove the domain string
            string pid = currentIdentity.Substring(currentIdentity.IndexOf("\\") + 1);

            return UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, pid);
        }

        public UserPrincipal GetUserByPid(string pid)
        {
            return UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, pid);
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

        public List<UserDTO> UserDetails(string emailAddress) 
        {
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