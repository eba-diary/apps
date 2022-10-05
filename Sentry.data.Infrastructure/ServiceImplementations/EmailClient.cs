using Sentry.data.Core;
using System.Net;
using System.Net.Mail;

namespace Sentry.data.Infrastructure
{
    //PURPOSE OF THIS CLASS IS TO CREATE A LAYER OF ABSTRACTION FROM SMTP CLIENT
    //SO WE CAN MOCK OUR EMAIL CLIENT AND PERFORM UNIT TESTING
    public class EmailClient : IEmailClient
    {
        public void Send(MailMessage mailMessage)
        {
            string smtpClientString = Configuration.Config.GetHostSetting(GlobalConstants.HostSettings.SMTPCLIENT);
            SmtpClient client = new SmtpClient(smtpClientString);
            client.Send(mailMessage);
        }
    }
}
