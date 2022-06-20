using Newtonsoft.Json;
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
    public class ConfluentConnectorService : IKafkaConnectorService
    {
        public ConfluentConnectorService()
        {

        }

        public ConfluentConnectorRootDTO GetConnectorDto(HttpResponseMessage resources)
        {
            string JsonString = resources.Content.ReadAsStringAsync().Result;
            ConfluentConnectorRootDTO connectorRootDTO = JsonConvert.DeserializeObject<ConfluentConnectorRootDTO>(JsonString);

            return connectorRootDTO;
        }
    }
}
