namespace Sentry.data.Core
{
    public interface IEmailService
    {
        void SendEmail(string toAddress, string subject, string body);
    }
}
