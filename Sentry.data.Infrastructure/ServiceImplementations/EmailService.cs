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

            smtpClient.Send("notifications@SentryData.com", toAddress, subject, body);

            System.Diagnostics.Trace.WriteLine("Sending email to " + toAddress + ". Subject = " + subject + "; Body = " + body);

        }
    }
}
