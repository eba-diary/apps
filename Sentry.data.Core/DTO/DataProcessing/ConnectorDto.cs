using Sentry.data.Core.GlobalEnums;

namespace Sentry.data.Core
{
    public class ConnectorDto
    {
        public string ConnectorName { get; set; }
        public ConnectorState ConnectorState { get; set; }
    }
}
