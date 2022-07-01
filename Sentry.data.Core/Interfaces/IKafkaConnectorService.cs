
using Newtonsoft.Json.Linq;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IKafkaConnectorService
    {
        Task<List<ConnectorDto>> GetS3ConnectorsDTOAsync();
        Task<JObject> GetS3ConnectorStatusJSONAsync(string connectorName);
        Task<JObject> GetS3ConnectorConfigJSONAsync(string connectorName);
    }
}
