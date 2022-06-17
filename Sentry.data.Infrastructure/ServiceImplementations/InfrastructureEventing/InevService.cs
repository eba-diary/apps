using RestSharp;
using RestSharp.Authenticators;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Interfaces.InfrastructureEventing;
using Sentry.data.Infrastructure.Exceptions;
using Sentry.data.Infrastructure.InfrastructureEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    /// <summary>
    /// Infrastructure Eventing Service - responsible for interacting with the REST Inev endpoint to publish/consume messages
    /// </summary>
    public class InevService : IInevService
    {
        private readonly IRestClient _restClient;

        public const string INEV_TOPIC = "INEV-DataLake";
        public const string INEV_MESSAGE_SOURCE = "DSC";
        public const string INEV_SAID_KEY = "DATA";
        public const string INEV_EVENTTYPE_PERMSUPDATED = "DatasetPermissionsUpdated";

        /// <summary>
        /// Public constructor
        /// </summary>
        public InevService(IRestClient restClient)
        {
            _restClient = restClient;
        }

        /// <summary>
        /// Publish a "DatasetPermissionsUpdated" event to Infrastructure Eventing
        /// </summary>
        /// <param name="dataset">The dataset whose permissions were updated</param>
        /// <param name="ticket">The ticket that was just approved</param>
        /// <param name="securablePermissions">The full list of securable permissions on this dataset</param>
        public async Task PublishDatasetPermissionsUpdated(Dataset dataset, SecurityTicket ticket, IList<SecurablePermission> securablePermissions)
        {
            var details = BuildDatasetPermissionsUpdatedDto(dataset, ticket, securablePermissions);
            await PublishInfrastructureEvent(INEV_EVENTTYPE_PERMSUPDATED, details.ToDictionary(), dataset.DatasetId.ToString());
        }

        /// <summary>
        /// Builds the DatasetPermissionsUpdated details object
        /// </summary>
        /// <param name="dataset">The dataset whose permissions were updated</param>
        /// <param name="ticket">The ticket that was just approved</param>
        /// <param name="securablePermissions">The full list of securable permissions on this dataset</param>
        private static DatasetPermissionsUpdatedDto BuildDatasetPermissionsUpdatedDto(Dataset dataset, SecurityTicket ticket, IList<SecurablePermission> securablePermissions)
        {

            //if this dataset has any schemas defined, grab the Snowflake info from the first one
            var snowflake = new List<DatasetPermissionsUpdatedDto.SnowflakeDto>();
            if (dataset.DatasetFileConfigs.Any())
            {
                var schema = dataset.DatasetFileConfigs.First().Schema;
                snowflake.Add(new DatasetPermissionsUpdatedDto.SnowflakeDto()
                {
                    Warehouse = schema.SnowflakeWarehouse,
                    Database = schema.SnowflakeDatabase,
                    Schema = schema.SnowflakeSchema,
                    Account = Configuration.Config.GetHostSetting("SnowAccount"),
                    SnowflakeType = SnowflakeConsumptionType.CategorySchemaParquet
                });
            }

            //get the dataset's full list of permissions
            var permissions = securablePermissions.Select(p =>
                new DatasetPermissionsUpdatedDto.PermissionDto()
                {
                    Identity = p.Identity,
                    IdentityType = p.IdentityType,
                    PermissionCode = p.SecurityPermission.Permission.PermissionCode,
                    Status = p.SecurityPermission.IsEnabled ? DatasetPermissionsUpdatedDto.PermissionDto.STATUS_ACTIVE : DatasetPermissionsUpdatedDto.PermissionDto.STATUS_REQUESTED
                }).ToList();

            //build up the changes specific to this ticket that was just approved
            var changes = new DatasetPermissionsUpdatedDto.ChangesDto()
            {
                Action = ticket.IsAddingPermission ? DatasetPermissionsUpdatedDto.ChangesDto.ACTION_ADD : DatasetPermissionsUpdatedDto.ChangesDto.ACTION_REMOVE,
                RequestedBy = ticket.RequestedById.ToString(),
                Permissions = ticket.Permissions.Select(p =>
                    new DatasetPermissionsUpdatedDto.PermissionDto()
                    {
                        Identity = ticket.Identity,
                        IdentityType = ticket.IdentityType,
                        PermissionCode = p.Permission.PermissionCode,
                        Status = p.IsEnabled ? DatasetPermissionsUpdatedDto.PermissionDto.STATUS_ACTIVE : DatasetPermissionsUpdatedDto.PermissionDto.STATUS_REQUESTED
                    }).ToList(),
            };

            //build the event details payload
            var details = new DatasetPermissionsUpdatedDto()
            {
                RequestId = ticket.SecurityTicketId.ToString(),
                DatasetId = dataset.DatasetId.ToString(),
                DatasetName = dataset.DatasetName,
                DatasetSaidKey = dataset.Asset.SaidKeyCode,
                DatasetNamedEnvironment = dataset.NamedEnvironment,
                DatasetNamedEnvironmentType = Enum.GetName(typeof(NamedEnvironmentType), dataset.NamedEnvironmentType),
                Snowflake = snowflake,
                Permissions = permissions,
                Changes = changes
            };
            return details;
        }

        /// <summary>
        /// Publishes an event to Infrastructure Eventing
        /// </summary>
        /// <param name="eventType">The Infrastructure Event Type</param>
        /// <param name="details">A dictionary that contains details about the event</param>
        /// <param name="id">An identifier for the entity this Infrastructure Event is related to (used solely for logging)</param>
        private async Task PublishInfrastructureEvent(string eventType, Dictionary<string, string> details, string id)
        {
            // Use RestSharp to make the call to the Infrastructure Eventing API.
            // This is because the response of the POST to /api/topics/ is a GUID string
            // However, the string is not enclosed in quotes, making it invalid JSON
            // The NSwag client chokes on this non-JSON value, requiring us to use this method
            // We still use the type ("Message") from the NSwag-generated code
            // See https://github.com/RicoSuter/NSwag/issues/2384
            _restClient.BaseUrl = new Uri(Configuration.Config.GetHostSetting("InfrastructureEventingServiceBaseUrl"));
            _restClient.Authenticator = new HttpBasicAuthenticator(Configuration.Config.GetHostSetting("ServiceAccountID"),
                                                    Configuration.Config.GetHostSetting("ServiceAccountPassword"));
            var request = new RestRequest() { Resource = "/api/topics/" };

            //serialize the request body ourselves, so we can specify the JsonSerializationOptions
            var jsonBody = System.Text.Json.JsonSerializer.Serialize(new Message()
            {
                Topic = INEV_TOPIC,
                MessageSource = INEV_MESSAGE_SOURCE,
                SaidKey = INEV_SAID_KEY,
                EventType = eventType,
                Details = details
            }, new System.Text.Json.JsonSerializerOptions() { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
            request.AddJsonBody(jsonBody);

            //execute the POST request
            var response = await _restClient.ExecuteTaskAsync(request, CancellationToken.None, Method.POST);

            //throw exception if there were errors executing the request
            if (response.ResponseStatus != ResponseStatus.Completed ||
                response.StatusCode != System.Net.HttpStatusCode.OK ||
                response.ErrorException != null)
            {
                throw new InfrastructureEventingException($"Error publishing '{eventType}' to Infrastructure Eventing for ID '{id}'. StatusCode={response.StatusCode}, ResponseStatus={response.ResponseStatus}", response.ErrorException);
            }
            Common.Logging.Logger.Debug($"'{eventType}' Infrastructure Event published for ID '{id}'. Response: {response.Content}");
        }

    }
}
