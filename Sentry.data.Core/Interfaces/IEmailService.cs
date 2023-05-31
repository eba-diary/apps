using System.Collections.Generic;
using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Core
{
    public interface IEmailService
    {
        void SendEmail(string emailAddress, string interval, string subject, List<Event> events);
        void SendInvalidReportLocationEmail(BusinessIntelligenceDto report, string userName);
        void SendGenericEmail(string emailAddress, string subject, string body, string cc);
        void SendS3SinkConnectorRequestEmail(DataFlow df, ConnectorCreateRequestDto requestDto, ConnectorCreateResponseDto responseDto);
        void SendNewMotiveTokenAddedEmail(DataSourceToken token);
        void SendMotiveDuplicateTokenEmail(DataSourceToken newToken, DataSourceToken oldToken);
        void SendConsumptionLayerStaleEmail(IList<SchemaConsumptionSnowflake> staleConsumptionLayers);
    }
}
