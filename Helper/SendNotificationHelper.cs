using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace BExIS.Web.Shell.Areas.RBM.Helpers
{
    public class SendNotificationHelper
    {
        public static void SendNotification(List<string> receiver, string sender, string message, string subject, bool isBodyHtml)
        {
            SmtpClient client = new SmtpClient();
            MailMessage mail = new MailMessage();
            mail.From =  new MailAddress(sender);
            mail.Subject = subject;
            mail.Body = message;
            mail.IsBodyHtml = isBodyHtml;
            foreach (string r in receiver)
            {
                mail.To.Add(r);
            }

            client.Send(mail);
        }
    }
}