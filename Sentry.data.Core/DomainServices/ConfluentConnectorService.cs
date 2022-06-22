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
    public class ConfluentConnectorService : IKafkaConnectorService
    {
        private IKafkaConnectorProvider _connectorProvider;

        public ConfluentConnectorService(IKafkaConnectorProvider connectorProvider)
        {
            _connectorProvider = connectorProvider;
        }

        public async Task<ConfluentConnectorRootDTO> GetConnectorDto(string resources)
        {
            HttpResponseMessage response = await _connectorProvider.GetRequestAsync("/connectors" + resources).ConfigureAwait(false);

            string JsonString = response.Content.ReadAsStringAsync().Result;

            ConfluentConnectorRootDTO connectorRootDTO = JsonConvert.DeserializeObject<ConfluentConnectorRootDTO>(JsonString);

            return connectorRootDTO;
        }

        public async Task<List<ConfluetConnectorRoot>> GetConnectorList()
        {
            List<string> connectorList = new List<string>();

            HttpResponseMessage response = await _connectorProvider.GetRequestAsync("/connectors").ConfigureAwait(false);

            string str = response.Content.ReadAsStringAsync().Result;

            var jArray = JArray.Parse(str);

            foreach(var j in jArray)
            {
                connectorList.Add(j.ToString());
            }

            return await GetConnectorDto(connectorList, "/connectors?expand=status&expand=info"); 
        }

        private async Task<List<ConfluetConnectorRoot>> GetConnectorDto(List<string> list, string resources)
        {
            HttpResponseMessage response = await _connectorProvider.GetRequestAsync(resources).ConfigureAwait(false);

            string ConnectorOjbectsList = response.Content.ReadAsStringAsync().Result;

            JObject JConnectorObjects = JObject.Parse(ConnectorOjbectsList);

            List<ConfluetConnectorRoot> confluetConnectorRoots = new List<ConfluetConnectorRoot>();

            list.ForEach(delegate (string connector)
            {
                ConfluetConnectorRoot confluetConnectorRoot = new ConfluetConnectorRoot()
                {
                    ConnectorName = connector
                };

                JToken jToken = JConnectorObjects.Property(connector);
                foreach (JToken items in jToken)
                {
                    // Get connector status data and set it the ConfluentConnecterRoot.ConflueConnetorStatus field
                    string statusToken = items.SelectToken("status").ToString();
                    ConfluentConnectorStatus connectorStatus = JsonConvert.DeserializeObject<ConfluentConnectorStatus>(statusToken);

                    confluetConnectorRoot.ConfluetConnectorStatus = connectorStatus;

                    // Get connector info data and set it the ConfluentConnecterRoot.ConflueConnetorInfo field
                    string infoToken = items.SelectToken("info").ToString();
                    ConfluentConnectorInfo connectorInfo = JsonConvert.DeserializeObject<ConfluentConnectorInfo>(infoToken);

                    confluetConnectorRoot.ConfluentConnectorInfo = connectorInfo;
                }

                confluetConnectorRoots.Add(confluetConnectorRoot);
            });

            return confluetConnectorRoots;
        }
    }
}
