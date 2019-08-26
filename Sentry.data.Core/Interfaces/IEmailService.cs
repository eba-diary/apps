using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IEmailService
    {
        void SendEmail(string emailAddress, string interval, string subject, List<Event> events);
        void SendInvalidReportLocationEmail(BusinessIntelligenceDto report, string userName);
        void SendGenericEmail(string emailAddress, string subject, string body, string cc);
    }
}
