using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Registry;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Interfaces;
using System.Collections.Generic;
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
            _asyncProviderPolicy = policyRegistry.Get<IAsyncPolicy>(PollyPolicyKeys.ApacheLivyProviderAsyncPolicy);
        }

        public string BaseUrl { get; set; } = "";


        public async Task<List<ConnectorRootDto>> GetS3Connectors()
        {
            HttpResponseMessage response = await GetRequestAsync("/connectors?expand=status&expand=info").ConfigureAwait(false);

            string ConnectorOjbectsList = response.Content.ReadAsStringAsync().Result;

            JObject JConnectorObjects = JObject.Parse(ConnectorOjbectsList);

            List<ConnectorRootDto> connectorRootDtos = await mapJsonToObject(JConnectorObjects);

            return connectorRootDtos;
        }

        private async Task<List<ConnectorRootDto>> mapJsonToObject(JObject jConnectorObjects)
        {
            List<ConfluentConnectorRoot> confluetConnectorRoots = new List<ConfluentConnectorRoot>();

            List<string> confluentConnectorNamelist = await getConnectorNameList();

            confluentConnectorNamelist.ForEach(delegate (string connectorName)
            {
                JToken jToken = jConnectorObjects.Property(connectorName);
                foreach (JToken items in jToken)
                {
                    // Get connector info data and set it the ConfluentConnecterRoot.ConflueConnetorInfo field
                    string infoToken = items.SelectToken("info").ToString();

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
                        string statusToken = items.SelectToken("status").ToString();

                        ConfluentConnectorStatus connectorStatus = JsonConvert.DeserializeObject<ConfluentConnectorStatus>(statusToken);

                        confluentConnectorRoot.ConfluetConnectorStatus = connectorStatus;

                        confluetConnectorRoots.Add(confluentConnectorRoot);
                    }   
                }
            });

            return MapToList(confluetConnectorRoots);
        }

        private async Task<List<string>> getConnectorNameList()
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

        private List<ConnectorRootDto> MapToList(List<ConfluentConnectorRoot> confluentConnectorRoots)
        {
            List<ConnectorRootDto> connectorRootDtos = new List<ConnectorRootDto>();

            confluentConnectorRoots.ForEach(ccr => connectorRootDtos.Add(MapToRootDto(ccr)));

            return connectorRootDtos;
        }

        private List<ConnectorTaskDto> MapToList(List<ConfluentConnectorStatusTask> confluentConnectorTasks)
        {
            List<ConnectorTaskDto> connectorTaskDtos = new List<ConnectorTaskDto>();

            confluentConnectorTasks.ForEach(cct => connectorTaskDtos.Add(MapToTaskDto(cct)));

            return connectorTaskDtos;
        }

        private ConnectorRootDto MapToRootDto(ConfluentConnectorRoot confluentConnectorRoot)
        {
            ConnectorRootDto connectorRootDto = new ConnectorRootDto();

            connectorRootDto.ConnectorName = confluentConnectorRoot.ConnectorName;
            connectorRootDto.ConnectorStatus = MapToStatusDto(confluentConnectorRoot.ConfluetConnectorStatus);
            connectorRootDto.ConnectorInfo = MapToInfoDto(confluentConnectorRoot.ConfluentConnectorInfo);


            return connectorRootDto;
        }

        private ConnectorStatusDto MapToStatusDto(ConfluentConnectorStatus confluentConnectorStatus)
        {
            ConnectorStatusDto connectorStatusDto = new ConnectorStatusDto();

            connectorStatusDto.Name = confluentConnectorStatus.name;
            connectorStatusDto.State = confluentConnectorStatus.connector.state;
            connectorStatusDto.WorkerId = confluentConnectorStatus.connector.worker_id;

            connectorStatusDto.ConnectorTasks = MapToList(confluentConnectorStatus.tasks);

            connectorStatusDto.Type = confluentConnectorStatus.type;

            return connectorStatusDto;
        }

        private ConnectorInfoDto MapToInfoDto(ConfluentConnectorInfo confluentConnectorInfo)
        {
            ConnectorInfoDto connectorInfoDto = new ConnectorInfoDto();

            connectorInfoDto.Name = confluentConnectorInfo.name;
            connectorInfoDto.Type = confluentConnectorInfo.type;
            connectorInfoDto.ConnectorClass = confluentConnectorInfo.confluentConnectorInfoConfig.ConnectorClass;
            connectorInfoDto.S3Region = confluentConnectorInfo.confluentConnectorInfoConfig.S3Region;
            connectorInfoDto.FlushSize = confluentConnectorInfo.confluentConnectorInfoConfig.FlushSize;
            connectorInfoDto.TasksMax = confluentConnectorInfo.confluentConnectorInfoConfig.TasksMax;
            connectorInfoDto.timezone = confluentConnectorInfo.confluentConnectorInfoConfig.timezone;
            connectorInfoDto.transforms = confluentConnectorInfo.confluentConnectorInfoConfig.transforms;
            connectorInfoDto.locale = confluentConnectorInfo.confluentConnectorInfoConfig.locale;
            connectorInfoDto.S3PathStyleAccessEnabled = confluentConnectorInfo.confluentConnectorInfoConfig.S3PathStyleAccessEnabled;
            connectorInfoDto.FormatClass = confluentConnectorInfo.confluentConnectorInfoConfig.FormatClass;
            connectorInfoDto.S3AclCanned = confluentConnectorInfo.confluentConnectorInfoConfig.S3AclCanned;
            connectorInfoDto.TransformsInsertMetadataPartitionField = confluentConnectorInfo.confluentConnectorInfoConfig.TransformsInsertMetadataPartitionField;
            connectorInfoDto.ValueConverter = confluentConnectorInfo.confluentConnectorInfoConfig.ValueConverter;
            connectorInfoDto.S3ProxyPassword = confluentConnectorInfo.confluentConnectorInfoConfig.S3ProxyPassword;
            connectorInfoDto.KeyConverter = confluentConnectorInfo.confluentConnectorInfoConfig.KeyConverter;
            connectorInfoDto.S3BucketName = confluentConnectorInfo.confluentConnectorInfoConfig.S3BucketName;
            connectorInfoDto.PartitionDurationMs = confluentConnectorInfo.confluentConnectorInfoConfig.PartitionDurationMs;
            connectorInfoDto.S3ProxyUser = confluentConnectorInfo.confluentConnectorInfoConfig.S3ProxyUser;
            connectorInfoDto.S3SseaName = confluentConnectorInfo.confluentConnectorInfoConfig.S3SseaName;
            connectorInfoDto.FileDelim = confluentConnectorInfo.confluentConnectorInfoConfig.FileDelim;
            connectorInfoDto.TransformsInsertMetadataOffsetField = confluentConnectorInfo.confluentConnectorInfoConfig.TransformsInsertMetadataOffsetField; ;
            connectorInfoDto.topics = confluentConnectorInfo.confluentConnectorInfoConfig.topics;
            connectorInfoDto.PartitionerClass = confluentConnectorInfo.confluentConnectorInfoConfig.PartitionerClass;
            connectorInfoDto.ValueConverterSchemasEnable = confluentConnectorInfo.confluentConnectorInfoConfig.ValueConverterSchemasEnable;
            connectorInfoDto.TransformsInsertMetadataTimestampField = confluentConnectorInfo.confluentConnectorInfoConfig.TransformsInsertMetadataTimestampField; ; ;
            connectorInfoDto.StorageClass = confluentConnectorInfo.confluentConnectorInfoConfig.StorageClass;
            connectorInfoDto.RotateScheduleIntervalMs = confluentConnectorInfo.confluentConnectorInfoConfig.RotateScheduleIntervalMs;
            connectorInfoDto.PathFormat = confluentConnectorInfo.confluentConnectorInfoConfig.PathFormat;
            connectorInfoDto.TimestampExtractor = confluentConnectorInfo.confluentConnectorInfoConfig.TimestampExtractor;
            connectorInfoDto.S3ProxyUrl = confluentConnectorInfo.confluentConnectorInfoConfig.S3ProxyUrl;
            connectorInfoDto.TransformsInsertMetadataType = confluentConnectorInfo.confluentConnectorInfoConfig.TransformsInsertMetadataType;

            return connectorInfoDto;
        }

        private ConnectorTaskDto MapToTaskDto(ConfluentConnectorStatusTask confluentConnectorTask)
        {
            ConnectorTaskDto connectorTaskDto = new ConnectorTaskDto()
            {
                Id = confluentConnectorTask.id,
                State = confluentConnectorTask.state,
                Worker_Id = confluentConnectorTask.worker_id
            };

            return connectorTaskDto;
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
