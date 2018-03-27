using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Net;
using System.Configuration;
using System.Web.Mvc;
using System.IO;
using System.Web.Routing;
using VORBS.Utils.interfaces;
using NLog;

namespace VORBS.Utils
{
    public class EmailHelper
    {
        private ISmtpClient mailClient;
        private HttpContextBase _context;
        private ILogger _logger;

        public EmailHelper(ISmtpClient mailClient, HttpContextBase context)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();

            this.mailClient = mailClient;
            _context = context;
            _logger.Trace(LoggerHelper.InitializeClassMessage());
        }

        /// <summary>
        /// Sends internal email
        /// </summary>
        /// <param name="fromAddress">E-Mail address of who the email is from</param>
        /// <param name="onBehalfOf">E-Mail address of who the E-Mail is onbehalf of</param>
        /// <param name="toAddress">E-Mail address of who will be recieving the email</param>
        /// <param name="subject">Subject line for E-Mail</param>
        /// <param name="body">Body of E-Mail (html-enabled)</param>
        public virtual void SendEmail(string fromAddress, string onBehalfOf, string toAddress, string subject, string body, bool bcc = true)
        {   
            MailMessage message = new MailMessage();
            message.From = new MailAddress(fromAddress);
            if (onBehalfOf != null)
            {
                message.From = new MailAddress(onBehalfOf);
                message.Sender = new MailAddress(fromAddress);
            }
            
            message.To.Add(new MailAddress(toAddress));
            message.Subject = subject;
            message.Body = body;

            if (bcc)
            {
                string bccEmail = ConfigurationManager.AppSettings["bccEmail"];
                if (bccEmail != "")
                    message.Bcc.Add(new MailAddress(bccEmail));
            }

            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(body, null, "text/html");

            htmlView.LinkedResources.Add(mailClient.GetLinkedResource(_context.Server.MapPath("~/Content/images/EmailTemplates/govuklogo.png"), "govuklogo"));
            htmlView.LinkedResources.Add(mailClient.GetLinkedResource(_context.Server.MapPath("~/Content/images/EmailTemplates/voalogo.png"), "voalogo"));

            message.AlternateViews.Add(htmlView);
            message.IsBodyHtml = true;
            try
            {
                mailClient.Send(message);

                _logger.Debug("MailClient sent message");
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, fromAddress, onBehalfOf, toAddress, subject, body, bcc));
            }
            catch (Exception ex)
            {
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(ex, fromAddress, onBehalfOf, toAddress, subject, body, bcc));
                throw ex;
            }
            
        }

        /// <summary>
        /// Sends internal email
        /// </summary>
        /// <param name="fromAddress">E-Mail address of who the email is from</param>
        /// <param name="toAddress">E-Mail address of who will be recieving the email</param>
        /// <param name="subject">Subject line for E-Mail</param>
        /// <param name="body">Body of E-Mail (html-enabled)</param>
        public virtual void SendEmail(string fromAddress, string toAddress, string subject, string body, bool bcc = true)
        {
            SendEmail(fromAddress, null, toAddress, subject, body, bcc);
        }

        /// <summary>
        /// Gets markup of a view based on its full path name and model
        /// </summary>
        /// <param name="viewName">Name of view to get markup for</param>
        /// <param name="model">Model to pass to view</param>
        /// <returns>HTML un-encoded markup of view</returns>
        public virtual string GetEmailMarkup(string viewName, object model)
        {
            ControllerContext controller = GetFakeContext();

            using (StringWriter sw = new StringWriter())
            {
                var razorViewEngine = new RazorViewEngine();
                var razorViewResult = razorViewEngine.FindView(controller, viewName, "", false);

                var viewContext = new ViewContext(controller, razorViewResult.View, new ViewDataDictionary(model), new TempDataDictionary(), sw);
                razorViewResult.View.Render(viewContext, sw);
                return sw.ToString();
            }
        }

        /// <summary>
        /// Creates an instance of a ControllerContext for spoofing controllers for a request
        /// </summary>
        /// <returns></returns>
        private static ControllerContext GetFakeContext()
        {
            var routeData = new RouteData();
            routeData.Values.Add("controller", "api");
            var httpContext = new HttpContext(new HttpRequest(null, "http://localhost:8080", null), new HttpResponse(null));
            var controller = new ControllerContext(new HttpContextWrapper(httpContext), routeData, new FakeController());

            return controller;
        }
        private class FakeController : ControllerBase { protected override void ExecuteCore() { } }
    }

}