using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.data.Core.DependencyInjection;
using Sentry.data.Core.DomainServices;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class ConnectorService : BaseDomainService<ConnectorService>, IKafkaConnectorService
    {
        private readonly IKafkaConnectorProvider _connectorProvider;
        
        public ConnectorService(IKafkaConnectorProvider connectorProvider, DomainServiceCommonDependency<ConnectorService> commonDepenencies) : base(commonDepenencies)
        {
            _connectorProvider = connectorProvider;
        }

        /// <summary>
        /// Goes to the Kafka Connector provider, retrieves a list of S3 ConnectorDto's and sorts them by Connector name.
        /// </summary>
        /// <returns>List of ConnectorDto's</returns>
        public async Task<List<ConnectorDto>> GetS3ConnectorsDTOAsync()
        {
            List<ConnectorDto> unsortedList = await _connectorProvider.GetS3ConnectorsAsync();

            List<ConnectorDto> sortedList = unsortedList.OrderBy(x=>x.ConnectorName).ToList();

            return sortedList;
        }

        /// <summary>
        /// Requests S3 Connector by it's name and retrieves a status JSON object
        /// </summary>
        /// <param name="connectorName">Name of S3 Connector to be returned</param>
        /// <returns>Specified JSON Connector status object</returns>
        public async Task<JObject> GetS3ConnectorStatusJSONAsync(string connectorName) 
        { 
            return await _connectorProvider.GetS3ConnectorStatusAsync(connectorName);
        }

        /// <summary>
        /// Requests S3 Connector by it's name and retrieves a config JSON object
        /// </summary>
        /// <param name="connectorName">Name of S3 Connector to be returned</param>
        /// <returns>Specified JSON Connector config object</returns>
        public async Task<JObject> GetS3ConnectorConfigJSONAsync(string connectorName)
        {
            JObject configJObj = await _connectorProvider.GetS3ConnectorConfigAsync(connectorName);

            // Remove sensitive key
            configJObj.Remove("s3.proxy.password");

            // Order object by ascending for readability purposes
            JObject returnObj = new JObject(configJObj.Properties().OrderBy(p => p.Name));

            return returnObj;
        }


        public async Task<ConnectorCreateResponseDto> CreateS3SinkConnectorAsync(ConnectorCreateRequestDto request)
        {
            string requestJSON = JsonConvert.SerializeObject(request);

            //IMPORTANT! Use ConfigureAwait(false) to essentially return to original session/thread that called this since caller of this needs the result
            HttpResponseMessage httpResponse = await _connectorProvider.CreateS3SinkConnectorAsync(requestJSON).ConfigureAwait(false);
            ConnectorCreateResponseDto responseDto = new ConnectorCreateResponseDto()
            {
                SuccessStatusCode = httpResponse.IsSuccessStatusCode,
                SuccessStatusCodeDescription = (httpResponse.IsSuccessStatusCode)? "SUCCESS" : "FAILED",
                StatusCode = ((int)httpResponse.StatusCode).ToString(),
                ReasonPhrase = httpResponse.ReasonPhrase
            };

            _logger.LogInformation($"Method <CreateS3SinkConnectorAsync> STATUS={responseDto.SuccessStatusCodeDescription} Creating: {requestJSON} ");
            return responseDto;
        }
    }
}
