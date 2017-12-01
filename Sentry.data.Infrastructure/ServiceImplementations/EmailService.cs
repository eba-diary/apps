using System.Net.Mail;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure
{
    public class EmailService : IEmailService
    {

        public void SendEmail(string toAddress, string subject, string body)
        {
            //Real code could look something like this:
            //Dim smtpClient As New System.Net.Mail.SmtpClient("mail.sentry.com")
            //smtpClient.Send("notifications@SentryData.com", toAddress, subject, body)

            SmtpClient smtpClient = new SmtpClient("mail.sentry.com");

            System.Diagnostics.Trace.WriteLine("Sending email to " + toAddress + ". Subject = " + subject + "; Body = " + body);


            // set smtp-client with basicAuthentication
            //mySmtpClient.UseDefaultCredentials = false;
            //System.Net.NetworkCredential basicAuthenticationInfo = new System.Net.NetworkCredential("username", "password");
            //mySmtpClient.Credentials = basicAuthenticationInfo;

            // add from,to mailaddresses
            MailAddress from = new MailAddress(Configuration.Config.GetHostSetting("DatasetMgmtEmail"));
            MailMessage myMail = new System.Net.Mail.MailMessage();
            myMail.From = from;

            myMail.IsBodyHtml = true;

            myMail.Body += @"<p><b><font color=""red"">Do Not Reply To This Email, This Inbox Is Not Monitored</font></b></p>";






        }
    }
}
