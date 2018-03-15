using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VORBS.Utils.interfaces;
using System.Net.Mail;
using System.Configuration;
using NLog;
using System.Text;

namespace VORBS.Utils
{
    public class EmailClient : ISmtpClient
    {
        string exchangeDomain = ConfigurationManager.AppSettings["exchangeDomain"];
        string userName = ConfigurationManager.AppSettings["emailUserName"];
        string password = ConfigurationManager.AppSettings["emailPassword"];
        string domain = ConfigurationManager.AppSettings["emailDomain"];

        private NLog.Logger _logger;

        public EmailClient()
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();

            _logger.Trace(LoggerHelper.InitializeClassMessage(
                    $"ExchangeDomain: {exchangeDomain}",
                    $"UserName: {userName}",
                    $"Password: *******",
                    $"Domain: {domain}"
                ));
        }

        public void Send(MailMessage message)
        {
            SmtpClient mailClient = new SmtpClient(exchangeDomain);
            _logger.Debug($"MailClient initialized");
            mailClient.UseDefaultCredentials = false;
            mailClient.Credentials = new System.Net.NetworkCredential(userName, password, domain);

            mailClient.DeliveryMethod = SmtpDeliveryMethod.Network;

            _logger.Debug($"MailClient configured");
            mailClient.Send(message);
            _logger.Debug($"MailClient sent. Message Details: {{ To:{message.To}, Sender:{message.Sender}, From:{message.From}, Subject(Length):{message.Subject.Length}, Body(Length):{message.Body.Length} }}");
            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, message));
        }

        public LinkedResource GetLinkedResource(string path, string id)
        {
            LinkedResource  resource = new LinkedResource(path);
            resource.ContentId = id;
            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(resource, path, id));
            return resource;
        }
    }
}