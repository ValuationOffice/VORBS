using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace VORBS.Utils.interfaces
{
    public interface ISmtpClient
    {
        void Send(MailMessage message);
        LinkedResource GetLinkedResource(string path, string id);
    }
}
