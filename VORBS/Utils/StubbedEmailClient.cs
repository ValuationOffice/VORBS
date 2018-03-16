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
            string rootDirectory = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Logs/{DateTime.Now.ToString("yyyy-MM-dd")}/EmailLogs/{message.To.FirstOrDefault().User}";
            if (!Directory.Exists(rootDirectory))
                Directory.CreateDirectory(rootDirectory);

            string fileName = $"{message.Subject}_{DateTime.Now.ToString("hh-mm-ss")}";
            using (FileStream fs = File.Create($"{rootDirectory}/{fileName}.html"))
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine($"From: {message.From}");
                builder.AppendLine($"To: {message.To}");
                builder.AppendLine($"BCC: {message.Bcc}");
                builder.AppendLine($"Subject: {message.Subject}");
                builder.AppendLine(message.Body);


                byte[] info = new UTF8Encoding(true).GetBytes(builder.ToString());
                fs.Write(info, 0, info.Length);

                // writing data in bytes already
                byte[] data = new byte[] { 0x0 };
                fs.Write(data, 0, data.Length);
            }
        }
    }
}