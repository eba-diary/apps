using System.Collections.Generic;
using System.Net.Mail;
using Sentry.data.Core;
using System.Linq;
using System.Text;

namespace Sentry.data.Infrastructure
{
    public class EmailService : IEmailService
    {

        public void SendEmail(string emailAddress, string subject, List<Event> events)
        {
            //Real code could look something like this:
            //Dim smtpClient As New System.Net.Mail.SmtpClient("mail.sentry.com")
            //smtpClient.Send("notifications@SentryData.com", toAddress, subject, body)

            SmtpClient smtpClient = new SmtpClient("mail.sentry.com");


            // set smtp-client with basicAuthentication
            //mySmtpClient.UseDefaultCredentials = false;
            //System.Net.NetworkCredential basicAuthenticationInfo = new System.Net.NetworkCredential("username", "password");
            //mySmtpClient.Credentials = basicAuthenticationInfo;

            // add from,to mailaddresses
            MailAddress from = new MailAddress(Configuration.Config.GetHostSetting("DatasetMgmtEmail"));
            MailMessage myMail = new System.Net.Mail.MailMessage();
            myMail.From = from;

            myMail.To.Add(emailAddress);

            myMail.IsBodyHtml = true;

            myMail.Body += @"<p><b><font color=""red"">Do Not Reply To This Email, This Inbox Is Not Monitored</font></b></p>";

            myMail.Body += @"<table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100 %"">";

            foreach (Event e in events.OrderBy(x => x.EventType.Severity))
            {
                myMail.Body += @"<tr>";

                myMail.Body += @"<td width=""300"" height=""120"" align=""left"" valign=""top"">";

                myMail.Body += e.Reason;

                myMail.Body += @"</td>";

                myMail.Body += @"<td width=""300"" height=""120"" align=""left"" valign=""top"">";

                myMail.Body += e.Status.Description;

                myMail.Body += @"</td>";

                myMail.Body += @"<td width=""300"" height=""120"" align=""left"" valign=""top"">";

                myMail.Body += e.EventType.Description;

                myMail.Body += @"</td>";

                myMail.Body += @" </tr>";
            }

            myMail.Body += @"</table>";
            
            smtpClient.Send(myMail);

        }
    }
}
