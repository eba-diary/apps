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
        private IKafkaConnectorProvider _connectorProvider;

        public ConnectorService(IKafkaConnectorProvider connectorProvider)
        {
            _connectorProvider = connectorProvider;
        }

        public async Task<List<ConnectorRootDto>> GetS3ConnectorsDTO()
        {
            return await _connectorProvider.GetS3Connectors();
        }
    }
}
