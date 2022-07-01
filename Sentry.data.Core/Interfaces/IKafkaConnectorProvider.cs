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
        Task<List<ConnectorDto>> GetS3ConnectorsAsync();
        Task<JObject> GetS3ConnectorStatusAsync(string connectorName);
        Task<JObject> GetS3ConnectorConfigAsync(string connectorName);
    }
}
