using Newtonsoft.Json.Linq;
using Sentry.data.Core.Entities.DataProcessing;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IKafkaConnectorProvider
    {
        Task<List<ConnectorRootDto>> GetS3Connectors();
        Task<JObject> GetS3ConnectorStatus(string connectorName);
        Task<JObject> GetS3ConnectorConfig(string connectorName);
    }
}
