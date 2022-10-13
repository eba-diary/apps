using System.Net.Mail;

namespace Sentry.data.Core
{
    //PURPOSE OF THIS INTERFACE IS TO ENFORCE A LAYER OF ABSTRACTION FROM SMTP CLIENT
    //SO WE CAN MOCK OUR EMAIL CLIENT AND PERFORM UNIT TESTING
    public interface IEmailClient
    {
        void Send(MailMessage mailMessage);
    }
}
