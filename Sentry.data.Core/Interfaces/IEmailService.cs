using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IEmailService
    {
        void SendEmail(string emailAddress, string interval, string subject, List<Event> events);
    }
}
