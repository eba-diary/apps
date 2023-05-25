using System.Collections.Generic;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class ConfluentConnector
    {
        public string ConnectorName { get; set; }
        public List<ConfluentConnectorTask> Tasks { get; set; }
    }
}
