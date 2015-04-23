using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace VORBS.Utils
{
    public class CustomHelpers
    {
        public static string GetUserNameFromDomain(string userName)
        {
            string domainSection = ConfigurationManager.AppSettings["domain"].ToString();
            string shortName = userName.Replace(domainSection, "");
            return shortName;
        }
    }
}