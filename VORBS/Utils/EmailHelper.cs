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

namespace VORBS.Utils
{
    public static class EmailHelper
    {
        /// <summary>
        /// Sends internal email
        /// </summary>
        /// <param name="fromAddress">E-Mail address of who the email is from</param>
        /// <param name="onBehalfOf">E-Mail address of who the E-Mail is onbehalf of</param>
        /// <param name="toAddress">E-Mail address of who will be recieving the email</param>
        /// <param name="subject">Subject line for E-Mail</param>
        /// <param name="body">Body of E-Mail (html-enabled)</param>
        public static void SendEmail(string fromAddress, string onBehalfOf, string toAddress, string subject, string body)
        {
            string exchangeDomain = ConfigurationManager.AppSettings["exchangeDomain"];
            string userName = ConfigurationManager.AppSettings["emailUserName"];
            string password = ConfigurationManager.AppSettings["emailPassword"];
            string domain = ConfigurationManager.AppSettings["emailDomain"];

            SmtpClient mailClient = new SmtpClient(exchangeDomain);
            mailClient.UseDefaultCredentials = false;
            mailClient.Credentials = new NetworkCredential(userName, password, domain);

            mailClient.DeliveryMethod = SmtpDeliveryMethod.Network;

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

            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(body, null, "text/html");

            htmlView.LinkedResources.Add(new LinkedResource(HttpContext.Current.Server.MapPath("~/Content/images/EmailTemplates/govuklogo.png")) { ContentId = "govuklogo" });
            htmlView.LinkedResources.Add(new LinkedResource(HttpContext.Current.Server.MapPath("~/Content/images/EmailTemplates/voalogo.png")) { ContentId = "voalogo" });

            message.AlternateViews.Add(htmlView);
            message.IsBodyHtml = true;

            mailClient.Send(message);
        }

        /// <summary>
        /// Sends internal email
        /// </summary>
        /// <param name="fromAddress">E-Mail address of who the email is from</param>
        /// <param name="toAddress">E-Mail address of who will be recieving the email</param>
        /// <param name="subject">Subject line for E-Mail</param>
        /// <param name="body">Body of E-Mail (html-enabled)</param>
        public static void SendEmail(string fromAddress, string toAddress, string subject, string body)
        {
            SendEmail(fromAddress, null, toAddress, subject, body);
        }

        /// <summary>
        /// Gets markup of a view based on its full path name and model
        /// </summary>
        /// <param name="viewName">Name of view to get markup for</param>
        /// <param name="model">Model to pass to view</param>
        /// <returns>HTML un-encoded markup of view</returns>
        public static string GetEmailMarkup(string viewName, object model)
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