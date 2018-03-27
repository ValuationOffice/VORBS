using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using VORBS.Utils.interfaces;

namespace VORBS.Utils
{
    public class StubbedEmailClient : ISmtpClient
    {
        private NLog.Logger _logger;

        public StubbedEmailClient()
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();

            _logger.Trace(LoggerHelper.InitializeClassMessage());
        }

        public LinkedResource GetLinkedResource(string path, string id)
        {
            LinkedResource resource = new LinkedResource(path);
            resource.ContentId = id;
            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(resource, path, id));
            return resource;
        }

        public void Send(MailMessage message)
        {
            SmtpClient client = new SmtpClient("stubbedhostname");
            client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;

            string rootDirectory = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Logs/{DateTime.Now.ToString("yyyy-MM-dd")}/EmailLogs/{message.To.FirstOrDefault().User}";
            if (!Directory.Exists(rootDirectory))
                Directory.CreateDirectory(rootDirectory);

            client.PickupDirectoryLocation = rootDirectory;
            client.Send(message);
        }
    }
}