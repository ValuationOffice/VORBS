using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Net;

namespace VORBS.Utils
{
    public static class Outlook
    {
        public static void SendMeetingInvite()
        {

        }

        public static void SendEmailInvite(string from, string to)
        {
            SmtpClient mailClient = new SmtpClient("devExchange.voaitdev.local", 443);
            MailMessage message = new MailMessage();

            mailClient.Credentials = new NetworkCredential("vorbsadmin", "Password12345", "voaitdev.local"); //Specfiy Credentials;
            //mailClient.UseDefaultCredentials = false;
            mailClient.Timeout = 10000; //5 Seconds
            mailClient.EnableSsl = true;
            mailClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            mailClient.TargetName = "SMTPSVC/devExchange.voaitdev.local";

            //mailClient = new SmtpClient();

            AdQueries queries = new AdQueries();

            from = queries.GetUserByPid(from).EmailAddress;
            to = queries.GetUserByPid(to).EmailAddress;


            message.To.Add(to);
            message.Subject = "Test";
            message.Body = "Email Sent From VORBS";
            message.From = new MailAddress(from);

            try
            {
                mailClient.Send(message);
                Console.Write("Sent");
            }
            catch (Exception ex)
            {
                //Log Exception
                Console.Write("Error");
            }
        }
    }
}