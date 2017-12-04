using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IEmailService
    {
        void SendEmail(List<string> emailAddresses, string subject, List<Event> events);
    }
}
