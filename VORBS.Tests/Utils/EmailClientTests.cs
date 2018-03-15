using NUnit.Framework;
using System;
using System.IO;
using System.Net.Mail;
using VORBS.Utils;
using VORBS.Utils.interfaces;

namespace VORBS.Tests
{
    [TestFixture]
    public class EmailClientTests
    {
        [Test]
        public void ShouldRetrieveLinkedResource()
        {
            ISmtpClient smtpClient = new EmailClient();
            var imageSource = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Utils\Images\govuklogo.png";
            var imageId = "someid";

            byte[] expectedBytes = StreamExtensions.GetBytesFromFile(imageSource);
            
            LinkedResource resource = smtpClient.GetLinkedResource(imageSource, imageId);
            byte[] actualBytes = resource.ContentStream.ReadAllBytes();

            Assert.AreEqual(imageId, resource.ContentId);
            Assert.AreEqual(expectedBytes, actualBytes);
        } 
    }

    internal static class StreamExtensions
    {
        public static byte[] GetBytesFromFile(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return fs.ReadAllBytes();
            }
        }

        public static byte[] ReadAllBytes(this Stream instream)
        {
            MemoryStream stream = null;

            if (instream is MemoryStream)
                stream = ((MemoryStream)instream);
            else
            {
                stream = new MemoryStream();
                instream.CopyTo(stream);
            }

            using (stream)
            {   
                byte[] bytes = stream.ToArray();
                stream.Close();
                stream.Dispose();
                return bytes;
            }
        }
    }
}
