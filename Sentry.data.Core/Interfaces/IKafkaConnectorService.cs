
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
        /// <summary>
        /// Goes to the Kafka Connector provider, retrieves a list of S3 ConnectorDto's and sorts them by Connector name.
        /// </summary>
        /// <returns>List of ConnectorDto's</returns>
        Task<List<ConnectorDto>> GetS3ConnectorsDTOAsync();

        /// <summary>
        /// Requests S3 Connector by it's name and retrieves a status JSON object
        /// </summary>
        /// <param name="connectorName">Name of S3 Connector to be returned</param>
        /// <returns>Specified JSON Connector status object</returns>
        Task<JObject> GetS3ConnectorStatusJSONAsync(string connectorName);

        /// <summary>
        /// Requests S3 Connector by it's name and retrieves a config JSON object
        /// </summary>
        /// <param name="connectorName">Name of S3 Connector to be returned</param>
        /// <returns>Specified JSON Connector config object</returns>
        Task<JObject> GetS3ConnectorConfigJSONAsync(string connectorName);

        Task<ConnectorCreateResponseDto> CreateS3SinkConnectorAsync(ConnectorCreateRequestDto request);

    }
}
