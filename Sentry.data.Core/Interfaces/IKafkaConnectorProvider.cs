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
        /// <summary>
        /// Requests all Confluent S3 Connectors
        /// </summary>
        /// <returns>List of ConnectorDto's</returns>
        Task<List<ConnectorDto>> GetS3ConnectorsAsync();

        /// <summary>
        /// Requests Connector status JSON object from Confluent API
        /// </summary>
        /// <param name="connectorName">Name of Connector to retrieve</param>
        /// <returns>Connector status JSON object</returns>
        Task<JObject> GetS3ConnectorStatusAsync(string connectorName);

        /// <summary>
        /// Requests Connector config JSON object from Confluent API
        /// </summary>
        /// <param name="connectorName">Name of Connector to retrieve</param>
        /// <returns>Connector config JSON object</returns>
        Task<JObject> GetS3ConnectorConfigAsync(string connectorName);
        Task<HttpResponseMessage> CreateS3SinkConnectorAsync(string requestJSON);
    }
}
