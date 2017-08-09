using System;
using VORBS;
using Moq;
using System.Net.Mail;
using System.Web;
using VORBS.Utils;
using VORBS.Utils.interfaces;
using System.IO;
using NUnit.Framework;

namespace VORBS.Tests
{
    [TestFixture]
    public class EmailHelperTests
    {
        [Test]
        public void ShouldSendDirectMail()
        {
            var smtpMock = new Mock<ISmtpClient>();
            var contextMock = new Mock<HttpContextBase>();

            string fromEmail = "fromEmail@email.com";
            string toEmail = "toEmail@email.com";
            string subject = "some subject";
            string body = "Some body";

            MailMessage result = null;
            smtpMock.Setup(x => x.Send(It.IsAny<MailMessage>()))
                .Callback<MailMessage>(r => result = r);

            //Set some fake image to be returned for embedded images
            var fakeImageData = Convert.FromBase64String("/9j/4AAQSkZJRgABAgEASABIAAD/2wBDAAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB");
            smtpMock.Setup(x => x.GetLinkedResource(It.IsAny<String>(), It.IsAny<String>()))
                .Returns(new LinkedResource(new MemoryStream(fakeImageData)));

            //Mock the context for accessing the resource
            contextMock.Setup(x => x.Server.MapPath(It.IsAny<string>()))
                .Returns("SomeImageData");

            EmailHelper helper = new EmailHelper(smtpMock.Object, contextMock.Object);

            //Ensure the send func for the EmailClient was used once
            helper.SendEmail(fromEmail, toEmail, subject, body, false);
            smtpMock.Verify(m => m.Send(It.IsAny<MailMessage>()), Times.Once);

            //Ensure the correct email properties are set
            Assert.AreEqual(fromEmail, result.From.Address.ToString());
            Assert.AreEqual(null, result.Sender);
            Assert.AreEqual(1, result.To.Count);
            Assert.AreEqual(subject, result.Subject);
            Assert.AreEqual(body, result.Body);
        }

        [Test]
        public void ShouldSendOnbehalfOfMail()
        {
            var smtpMock = new Mock<ISmtpClient>();
            var contextMock = new Mock<HttpContextBase>();

            string fromEmail = "fromEmail@email.com";
            string onbehalfOfEmail = "onbehalfOfEmail@email.com";
            string toEmail = "toEmail@email.com";
            string subject = "some subject";
            string body = "Some body";

            MailMessage result = null;
            smtpMock.Setup(x => x.Send(It.IsAny<MailMessage>()))
                .Callback<MailMessage>(r => result = r);

            //Set some fake image to be returned for embedded images
            var fakeImageData = Convert.FromBase64String("/9j/4AAQSkZJRgABAgEASABIAAD/2wBDAAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB");
            smtpMock.Setup(x => x.GetLinkedResource(It.IsAny<String>(), It.IsAny<String>()))
                .Returns(new LinkedResource(new MemoryStream(fakeImageData)));

            //Mock the context for accessing the resource
            contextMock.Setup(x => x.Server.MapPath(It.IsAny<string>()))
                .Returns("SomeImageData");

            EmailHelper helper = new EmailHelper(smtpMock.Object, contextMock.Object);

            //Ensure the send func for the EmailClient was used once
            helper.SendEmail(fromEmail, onbehalfOfEmail, toEmail, subject, body, false);
            smtpMock.Verify(m => m.Send(It.IsAny<MailMessage>()), Times.Once);

            //Ensure the correct email properties are set
            Assert.AreEqual(onbehalfOfEmail, result.From.Address.ToString());
            Assert.AreEqual(fromEmail, result.Sender.Address.ToString());
            Assert.AreEqual(1, result.To.Count);
            Assert.AreEqual(subject, result.Subject);
            Assert.AreEqual(body, result.Body);
        }
    }
}
