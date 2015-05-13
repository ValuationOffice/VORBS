using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Permissions;
using System.Web;

namespace VORBS.AD
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
    }
}