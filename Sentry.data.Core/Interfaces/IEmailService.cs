using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IEmailService
    {
        void SendEmail(string emailAddress, string subject, List<Event> events);
    }
}
