using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.Configuration;
using Sentry.data.Core.Entities;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class ConnectorService : IKafkaConnectorService
    {
        private readonly IKafkaConnectorProvider _connectorProvider;

        public ConnectorService(IKafkaConnectorProvider connectorProvider)
        {
            _connectorProvider = connectorProvider;
        }

        /// <summary>
        /// Goes to Kafka Connector provider, retrieves a list of S3 ConnectorRootDto's and sorts them by Connector name.
        /// </summary>
        /// <returns>List of ConnectorRootDto's</returns>
        public async Task<List<ConnectorRootDto>> GetS3ConnectorsDTO()
        {
            List<ConnectorRootDto> unsortedList = await _connectorProvider.GetS3Connectors();

            List<ConnectorRootDto> sortedList = unsortedList.OrderBy(x=>x.ConnectorName).ToList();

            return sortedList;
        }

        /// <summary>
        /// Requests S3 Connector by it's name and retrieves S3 Cconnector Status JSON object
        /// </summary>
        /// <param name="connectorName">Name of S3 connector to be returned</param>
        /// <returns>Specified JSON connector status object</returns>
        public async Task<JObject> GetS3ConnectorStatusJSON(string connectorName) 
        { 
            return await _connectorProvider.GetS3ConnectorStatus(connectorName);
        }

        /// <summary>
        /// Requests S3 Connector by it's name and retrieves S3 Cconnector Config JSON object
        /// </summary>
        /// <param name="connectorName">Name of S3 connector to be returned</param>
        /// <returns>Specified JSON Connector Config object</returns>
        public async Task<JObject> GetS3ConnectorConfigJSON(string connectorName)
        {
            return await _connectorProvider.GetS3ConnectorConfig(connectorName);
        }
    }
}
