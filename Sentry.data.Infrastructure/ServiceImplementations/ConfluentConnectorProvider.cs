using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Registry;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class ConfluentConnectorProvider : IKafkaConnectorProvider
    {
        private readonly IAsyncPolicy _asyncProviderPolicy;
        private readonly IHttpClientProvider _httpClient;
        private readonly string _baseUrl;

        public ConfluentConnectorProvider(IHttpClientProvider httpClientProvider, IPolicyRegistry<string> policyRegistry, string baseUrl)
        {
            _httpClient = httpClientProvider;
            _asyncProviderPolicy = policyRegistry.Get<IAsyncPolicy>(PollyPolicyKeys.ConfluentConnectorProviderAsyncPolicy);
            _baseUrl = baseUrl;
        }

        /// <summary>
        /// Requests all Confluent S3 Connectors
        /// </summary>
        /// <returns>List of ConnectorDto's</returns>
        public async Task<List<ConnectorDto>> GetS3ConnectorsAsync()
        {
            JObject JConnectorObjects = await RequestConfluentJsonAsync("/connectors?expand=status&expand=info");

            List<ConnectorDto> connectorDtos = MapJsonToDtos(JConnectorObjects);

            return connectorDtos;
        }

        /// <summary>
        /// Requests Connector status JSON object from Confluent API
        /// </summary>
        /// <param name="connectorName">Name of Connector to retrieve</param>
        /// <returns>Connector status JSON object</returns>
        public async Task<JObject> GetS3ConnectorStatusAsync(string connectorName)
        {
            return await RequestConfluentJsonAsync($"/connectors/{connectorName}/status");
        }

        public async Task<HttpResponseMessage> CreateS3SinkConnectorAsync(string requestJSON)
        {
            StringContent stringContent = new StringContent(requestJSON, System.Text.Encoding.UTF8, "application/json");
            string baseURLplusConnectorEndpoint = $"{_baseUrl}/connectors/";

            //IMPORTANT! Use ConfigureAwait(false) to essentially return to original session/thread that called this since caller of this needs the result
            HttpResponseMessage httpResponse = await _httpClient.PostAsync(baseURLplusConnectorEndpoint, stringContent).ConfigureAwait(false);
            return httpResponse;
        }



        /// <summary>
        /// Requests Connector config JSON object from Confluent API
        /// </summary>
        /// <param name="connectorName">Name of Connector to retrieve</param>
        /// <returns>Connector config JSON object</returns>
        public async Task<JObject> GetS3ConnectorConfigAsync(string connectorName)
        {
            return await RequestConfluentJsonAsync($"/connectors/{connectorName}/config");
        }


        /// <summary>
        /// Requests Confluent API for HttpResponseMessage 
        /// </summary>
        /// <param name="resource">Specified request</param>
        /// <returns>JSON object returned from passes in request</returns>
        private async Task<JObject> RequestConfluentJsonAsync(string resource)
        {
            HttpResponseMessage response = await GetRequestAsync(resource).ConfigureAwait(false);

            string connectorObjectsList = response.Content.ReadAsStringAsync().Result;

            return JObject.Parse(connectorObjectsList);
        }

        private List<ConnectorDto> MapJsonToDtos(JObject jConnectorObjects)
        {
            List<ConfluentConnector> confluentConnectorList = new List<ConfluentConnector>();

            //Iterates over list of jObjects
            foreach (JToken currentToken in jConnectorObjects.Children())
            {
                JToken connectorToken = currentToken.Children().First();

                //Checks if the current connector class is set to io.confluent.connect.s3.S3SinkConnector
                if ((connectorToken.SelectToken("info.config.['connector.class']").ToString() == "io.confluent.connect.s3.S3SinkConnector")) 
                {
                    //Get the status json string from current jToken
                    string tasksToken = connectorToken.SelectToken("status.tasks").ToString();

                    //Create and set the ConfluentConnectorRoot object
                    ConfluentConnector confluentConnectorRoot = new ConfluentConnector()
                    {
                        ConnectorName = currentToken.First.Path,
                        Tasks = JsonConvert.DeserializeObject<List<ConfluentConnectorTask>>(tasksToken)
                    };

                    confluentConnectorList.Add(confluentConnectorRoot);
                }
            }

            return MapToList(confluentConnectorList);
        }

        private List<ConnectorDto> MapToList(List<ConfluentConnector> confluentConnectors)
        {
            List<ConnectorDto> connectorDtos = new List<ConnectorDto>();

            confluentConnectors.ForEach(ccr => connectorDtos.Add(MapToRootDto(ccr)));

            return connectorDtos;
        }

        private ConnectorDto MapToRootDto(ConfluentConnector confluentConnectors)
        {
            ConnectorDto connectorDto = new ConnectorDto();

            connectorDto.ConnectorName = confluentConnectors.ConnectorName;

            int connectorRunningTaskCount = 0;

            //Counts the amount of running Connector Tasks
            foreach (ConfluentConnectorTask task in confluentConnectors.Tasks)
            {
                if (task.State == ConnectorState.RUNNING.ToString()) connectorRunningTaskCount++;
            }

            //Checks if all Connector Tasks are running
            if (confluentConnectors.Tasks.All(x => x.State == ConnectorState.RUNNING.ToString()))
            {
                connectorDto.ConnectorState = ConnectorState.RUNNING;
            }
            //Checks if all of the Connector Tasks have failed
            else if (!confluentConnectors.Tasks.Any(x => x.State == ConnectorState.RUNNING.ToString()))
            {
                connectorDto.ConnectorState = ConnectorState.FAILED;
            }
            //Checks if a portion of the Connector Tasks have failed
            else
            {
                connectorDto.ConnectorState = ConnectorState.DEGRADED;
            }

            return connectorDto;
        }

        private async Task<HttpResponseMessage> GetRequestAsync(string resource)
        {
            var pollyResponse = await _asyncProviderPolicy.ExecuteAsync(async () =>
            {
                var x = await _httpClient.GetAsync(_baseUrl + resource).ConfigureAwait(false);

                return x;

            }).ConfigureAwait(false);

            HttpResponseMessage response = pollyResponse;

            return response;
        }
    }
}
