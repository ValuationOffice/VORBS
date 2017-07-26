using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VORBS.Utils.interfaces;
using System.Net.Mail;
using System.Configuration;

namespace VORBS.Utils
{
    public class EmailClient : ISmtpClient
    {
        string exchangeDomain = ConfigurationManager.AppSettings["exchangeDomain"];
        string userName = ConfigurationManager.AppSettings["emailUserName"];
        string password = ConfigurationManager.AppSettings["emailPassword"];
        string domain = ConfigurationManager.AppSettings["emailDomain"];

        public void Send(MailMessage message)
        {
            SmtpClient mailClient = new SmtpClient(exchangeDomain);
            mailClient.UseDefaultCredentials = false;
            mailClient.Credentials = new System.Net.NetworkCredential(userName, password, domain);

            mailClient.DeliveryMethod = SmtpDeliveryMethod.Network;

            mailClient.Send(message);
        }

        public LinkedResource GetLinkedResource(string path, string id)
        {
            return new LinkedResource(path, id);
        }
    }
}