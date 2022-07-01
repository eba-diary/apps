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

        public ConfluentConnectorProvider(IHttpClientProvider httpClientProvider, IPolicyRegistry<string> policyRegistry)
        {
            _httpClient = httpClientProvider;
            _asyncProviderPolicy = policyRegistry.Get<IAsyncPolicy>(PollyPolicyKeys.ConfluentConnectorProviderAsyncPolicy);
        }

        public string BaseUrl { get; set; } = "";

        /// <summary>
        /// Requests all Confluent S3 Connectors
        /// </summary>
        /// <returns>List of ConnectorDto's</returns>
        public async Task<List<ConnectorDto>> GetS3ConnectorsAsync()
        {
            JObject JConnectorObjects = await requestConfluentJsonAsync("/connectors?expand=status&expand=info");

            List<ConnectorDto> connectorDtos = await mapJsonToDtosAsync(JConnectorObjects);

            return connectorDtos;
        }

        /// <summary>
        /// Requests Connector Status JSON object from Confluent API
        /// </summary>
        /// <param name="connectorName">Name of Connector to retrieve</param>
        /// <returns>Connector Status JSON object</returns>
        public async Task<JObject> GetS3ConnectorStatusAsync(string connectorName)
        {
            return await requestConfluentJsonAsync($"/connectors/{connectorName}/status");
        }

        /// <summary>
        /// Requests Connector Config JSON object from Confluent API
        /// </summary>
        /// <param name="connectorName">Name of Connector to retrieve</param>
        /// <returns>Connector Config JSON object</returns>
        public async Task<JObject> GetS3ConnectorConfigAsync(string connectorName)
        {
            return await requestConfluentJsonAsync($"/connectors/{connectorName}/config");
        }

        /// <summary>
        /// Requests Confluent API for HttpResponseMessage 
        /// </summary>
        /// <param name="resource">Specified request</param>
        /// <returns>JSON object returned from passes in request</returns>
        private async Task<JObject> requestConfluentJsonAsync(string resource)
        {
            HttpResponseMessage response = await GetRequestAsync(resource).ConfigureAwait(false);

            string connectorOjbectsList = response.Content.ReadAsStringAsync().Result;

            return JObject.Parse(connectorOjbectsList);
        }

        private async Task<List<ConnectorDto>> mapJsonToDtosAsync(JObject jConnectorObjects)
        {
            List<ConfluentConnectorRoot> confluetConnectorRoots = new List<ConfluentConnectorRoot>();

            List<string> confluentConnectorNameList = await getConnectorNameListAsync();

            //Iterates over entire list of confluent connector names returned from API
            confluentConnectorNameList.ForEach(delegate (string connectorName)
            {
                //Parses passed in JSON object and returns JToken of specified connector  
                JToken jToken = jConnectorObjects.Property(connectorName).Children().First();

                // Get connector info data and set it the ConfluentConnecterRoot.ConflueConnetorInfo field
                string infoToken = jToken.SelectToken("info").ToString();

                // Checks if the connector class is of type io.confluent.connect.s3.S3SinkConnector, if not, the connector will be skipped and not added to the list
                if (infoToken.Contains("io.confluent.connect.s3.S3SinkConnector"))
                {
                    ConfluentConnectorRoot confluentConnectorRoot = new ConfluentConnectorRoot()
                    {
                        ConnectorName = connectorName
                    };

                    ConfluentConnectorInfo connectorInfo = JsonConvert.DeserializeObject<ConfluentConnectorInfo>(infoToken);

                    confluentConnectorRoot.ConfluentConnectorInfo = connectorInfo;

                    // Get connector status data and set it the ConfluentConnecterRoot.ConflueConnetorStatus field
                    string statusToken = jToken.SelectToken("status").ToString();

                    ConfluentConnectorStatus connectorStatus = JsonConvert.DeserializeObject<ConfluentConnectorStatus>(statusToken);

                    confluentConnectorRoot.ConfluetConnectorStatus = connectorStatus;

                    confluetConnectorRoots.Add(confluentConnectorRoot);
                }   
            });

            return MapToList(confluetConnectorRoots);
        }

        /// <summary>
        /// Request list of all Connector names from Confluent API
        /// </summary>
        /// <returns>String List of Connector Names</returns>
        private async Task<List<string>> getConnectorNameListAsync()
        {
            List<string> connectorNameList = new List<string>();

            HttpResponseMessage response = await GetRequestAsync("/connectors").ConfigureAwait(false);

            string jsonString = response.Content.ReadAsStringAsync().Result;

            var jArray = JArray.Parse(jsonString);

            foreach (var j in jArray)
            {
                connectorNameList.Add(j.ToString());
            }

            return connectorNameList;
        }

        private List<ConnectorDto> MapToList(List<ConfluentConnectorRoot> confluentConnectorRoots)
        {
            List<ConnectorDto> connectorDtos = new List<ConnectorDto>();

            confluentConnectorRoots.ForEach(ccr => connectorDtos.Add(MapToRootDto(ccr)));

            return connectorDtos;
        }

        private ConnectorDto MapToRootDto(ConfluentConnectorRoot confluentConnectorRoot)
        {
            ConnectorDto connectorDto = new ConnectorDto();

            connectorDto.ConnectorName = confluentConnectorRoot.ConnectorName;

            int connectorRunningTaskCount = 0;

            //Counts the amount of running Connector Tasks
            foreach (ConfluentConnectorStatusTask task in confluentConnectorRoot.ConfluetConnectorStatus.Tasks)
            {
                if (task.state == ConnectorStateEnum.RUNNING.ToString()) connectorRunningTaskCount++;
            }

            //Checks if all Connector Tasks are running
            if (connectorRunningTaskCount == confluentConnectorRoot.ConfluetConnectorStatus.Tasks.Count)
            {
                connectorDto.ConnectorState = ConnectorStateEnum.RUNNING;
            }
            //Checks if all of the Connector Tasks have failed
            else if (connectorRunningTaskCount == 0)
            {
                connectorDto.ConnectorState = ConnectorStateEnum.FAILED;
            }
            //Checks if a portion of the Connector Tasks have failed
            else if (connectorRunningTaskCount > 0 || connectorRunningTaskCount < confluentConnectorRoot.ConfluetConnectorStatus.Tasks.Count)
            {
                connectorDto.ConnectorState = ConnectorStateEnum.DEGRADED;
            }

            return connectorDto;
        }

        private async Task<HttpResponseMessage> GetRequestAsync(string resource)
        {
            var pollyResponse = await _asyncProviderPolicy.ExecuteAsync(async () =>
            {
                var x = await _httpClient.GetAsync(BaseUrl + resource).ConfigureAwait(false);

                return x;

            }).ConfigureAwait(false);

            HttpResponseMessage response = pollyResponse;

            return response;
        }

        private async Task<HttpResponseMessage> PostRequestAsync(string resource, HttpContent postContent)
        {
            var pollyResponse = await _asyncProviderPolicy.ExecuteAsync(async () =>
            {
                var x =  await _httpClient.PostAsync(BaseUrl + $"/{resource}", postContent).ConfigureAwait(false);

                return x;

            }).ConfigureAwait(false);

             HttpResponseMessage response = pollyResponse;

            return response;
        }

        private async Task<HttpResponseMessage> DeleteRequestAsync(string resource)
        {
            var pollyResponse = await _asyncProviderPolicy.ExecuteAsync(async () =>
            {
                var x = await _httpClient.DeleteAsync(BaseUrl + resource).ConfigureAwait(false);

                return x;

            }).ConfigureAwait(false);

            HttpResponseMessage response = pollyResponse;

            return response;
        }
    }
}
