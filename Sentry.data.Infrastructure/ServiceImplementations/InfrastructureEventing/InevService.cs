using RestSharp;
using RestSharp.Authenticators;
using Sentry.Configuration;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Interfaces.InfrastructureEventing;
using Sentry.data.Infrastructure.Exceptions;
using Sentry.data.Infrastructure.InfrastructureEvents;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Infrastructure
{
    /// <summary>
    /// Infrastructure Eventing Service - responsible for interacting with the REST Inev endpoint to publish/consume messages
    /// </summary>
    public class InevService : IInevService
    {
        private readonly IRestClient _restClient;
        private readonly IClient _inevClient;
        private readonly IDatasetContext _datasetContext;

        public const string INEV_TOPIC = "INEV-DataLake"; //DSC-Debug is a listener on it 
        public const string INEV_TOPIC_DBA_PORTAL_COMPLETE = "INEV-DBAPortalRequestComplete";
        public const string INEV_TOPIC_DBA_PORTAL_APPROVED = "INEV-DBAPortalRequestApproved";
        public const string INEV_TOPIC_DBA_PORTAL_ADDED = "INEV-DBAPortalTicketAdded";
        public string INEV_GROUP_DSC_CONSUMER = Config.GetHostSetting("INEV-DATA-CONSUMER");
        public const string INEV_MESSAGE_SOURCE = "DSC";
        public const string INEV_SAID_KEY = "DATA";
        public const string INEV_EVENTTYPE_PERMSUPDATED = "DatasetPermissionsUpdated";

        /// <summary>
        /// Public constructor
        /// </summary>
        public InevService(IRestClient restClient, IClient inevClient, IDatasetContext datasetContext)
        {
            _restClient = restClient;
            _inevClient = inevClient;
            _datasetContext = datasetContext;
        }

        /// <summary>
        /// Publish a "DatasetPermissionsUpdated" event to Infrastructure Eventing
        /// </summary>
        /// <param name="dataset">The dataset whose permissions were updated</param>
        /// <param name="ticket">The ticket that was just approved</param>
        /// <param name="datasetPermissions">The full list of securable permissions on this dataset</param>
        public async Task PublishDatasetPermissionsUpdated(Dataset dataset, SecurityTicket ticket, IList<SecurablePermission> datasetPermissions, IList<SecurablePermission> parentPermissions)
        {
            var details = BuildDatasetPermissionsUpdatedDto(dataset, ticket, datasetPermissions, parentPermissions);
            var datasetId = dataset.DatasetId.ToString();
            var payload = details.ToDictionary();
            await PublishInfrastructureEvent(INEV_EVENTTYPE_PERMSUPDATED, payload, datasetId);
            //if snowflake permissions were just requested from DBA, set ticket to pending DBA to be tracked through INEV
            //Need these to be toggled on and off so we sent the request to DBA in correct state
            if(ticket.TicketStatus == HpsmTicketStatus.COMPLETED && ticket.AddedPermissions.Any(p => p.Permission.PermissionCode == PermissionCodes.SNOWFLAKE_ACCESS) || ticket.RemovedPermissions.Any(p => p.Permission.PermissionCode == PermissionCodes.SNOWFLAKE_ACCESS))
            {
                ticket.TicketStatus = DbaFlowTicketStatus.DbaTicketPending;
                ticket.AddedPermissions?.Where(p => p.Permission.PermissionCode == PermissionCodes.SNOWFLAKE_ACCESS).ToList().ForEach(x =>
                {
                    x.IsEnabled = false;
                    x.EnabledDate = null;
                });
                ticket.RemovedPermissions?.Where(p => p.Permission.PermissionCode == PermissionCodes.SNOWFLAKE_ACCESS).ToList().ForEach(x =>
                {
                    x.IsEnabled = true;
                    x.RemovedDate = null;
                });   
            }
        }

        /// <summary>
        /// Check Infrastructure Eventing topics for notification that the DBA Portal created a Cherwell ticket for a Snowflake access request
        /// </summary>
        public async Task CheckDbaPortalEvents()
        {
            //Only is relevant to snowflake permissions right now
            //Grab events for ticket added topic - 
            
            //  Grab events where SourceRequestId is not null
            //    Match Source Request ID with SecurityTicketId
            //      Add DBA Request ID to SecurityTicket row
            //        Ticket is still pending

            //Grab events for ticket approved
            //  Match events to tickets in pending based on DBA Request ID
            //    Update ticket to approved pending processing

            //Grab events for ticket complete
            //  Match ticket complete events based on DBA Request ID
            //    Mark ticket and perms active 





            try
            {
                Sentry.Common.Logging.Logger.Info("Checking for Infrastructure Events to Consume: ");

                var addedMessages = _inevClient.ConsumeGroupUsingGETAsync(INEV_TOPIC_DBA_PORTAL_ADDED, INEV_GROUP_DSC_CONSUMER, 20).Result.Messages.ToList();

                Sentry.Common.Logging.Logger.Info($"Found {addedMessages.Count()} DBA Portal Added Events to Consume");

                foreach (var message in addedMessages)
                {
                    if (message.Details.TryGetValue("SourceRequest_ID", out string sourceRequestID))
                    {
                        Guid sourceGuid = new Guid(sourceRequestID);
                        var sourceTicket = _datasetContext.SecurityTicket.Where(t => t.SecurityTicketId.Equals(sourceGuid)).First();
                        if (sourceTicket != null)
                        {
                            var requestId = message.Details.TryGetValue("RequestID", out string dbaRequestId);
                            sourceTicket.ExternalRequestId = dbaRequestId;
                            sourceTicket.TicketStatus = GlobalConstants.DbaFlowTicketStatus.DbaTicketAdded;
                        }
                    }
                }

                var approvedMessages = _inevClient.ConsumeGroupUsingGETAsync(INEV_TOPIC_DBA_PORTAL_APPROVED, INEV_GROUP_DSC_CONSUMER, 20).Result.Messages.ToList();

                Sentry.Common.Logging.Logger.Info($"Found {approvedMessages.Count()} DBA Portal Approved Events to Consume");

                foreach (var message in approvedMessages)
                {
                    if (message.Details.TryGetValue("RequestID", out string dbaRequestId))
                    {
                        var sourceTicket = _datasetContext.SecurityTicket.Where(t => t.ExternalRequestId.Equals(dbaRequestId)).First();
                        if (sourceTicket != null)
                        {
                            sourceTicket.TicketStatus = GlobalConstants.DbaFlowTicketStatus.DbaTicketApproved;
                        }
                    }
                }

                var completedMessages = _inevClient.ConsumeGroupUsingGETAsync(INEV_TOPIC_DBA_PORTAL_COMPLETE, INEV_GROUP_DSC_CONSUMER, 20).Result.Messages.ToList();

                Sentry.Common.Logging.Logger.Info($"Found {completedMessages.Count()} DBA Portal Completed Events to Consume");

                foreach (var message in completedMessages)
                {
                    if (message.Details.TryGetValue("RequestID", out string dbaRequestId))
                    {
                        var sourceTicket = _datasetContext.SecurityTicket.Where(t => t.ExternalRequestId.Equals(dbaRequestId)).First();
                        if (sourceTicket != null)
                        {
                            sourceTicket.TicketStatus = GlobalConstants.DbaFlowTicketStatus.DbaTicketComplete;
                            if (sourceTicket.IsAddingPermission)
                            {
                                sourceTicket.AddedPermissions.ToList().ForEach(x =>
                                {
                                    x.IsEnabled = true;
                                    x.EnabledDate = DateTime.Now;
                                });
                            }
                            else
                            {
                                List<SecurityPermission> toRemove = _datasetContext.SecurityPermission.Where(p => p.RemovedFromTicket == sourceTicket).ToList();
                                toRemove.ForEach(x =>
                                {
                                    x.IsEnabled = false;
                                    x.RemovedDate = DateTime.Now;
                                });
                            }
                        }
                    }
                }

                _datasetContext.SaveChanges();
            }
            catch (Exception e)
            {
                Sentry.Common.Logging.Logger.Fatal("Error while trying to consume events.", e);
            }
        }

        /// <summary>
        /// Builds the DatasetPermissionsUpdated details object
        /// </summary>
        /// <param name="dataset">The dataset whose permissions were updated</param>
        /// <param name="ticket">The ticket that was just approved</param>
        /// <param name="securablePermissions">The full list of securable permissions on this dataset</param>
        private static DatasetPermissionsUpdatedDto BuildDatasetPermissionsUpdatedDto(Dataset dataset, SecurityTicket ticket, IList<SecurablePermission> datasetPermissions, IList<SecurablePermission> parentPermissions)
        {

            //if this dataset has any schemas defined, grab the Snowflake info from the first one
            var snowflake = new List<DatasetPermissionsUpdatedDto.SnowflakeDto>();
            if (dataset.DatasetFileConfigs.Any())
            {
                snowflake.AddRange(
                    dataset.DatasetFileConfigs.First(dsfc => dsfc.ObjectStatus == ObjectStatusEnum.Active).Schema.ConsumptionDetails.OfType<SchemaConsumptionSnowflake>().Select(s =>
                        new DatasetPermissionsUpdatedDto.SnowflakeDto()
                        {
                            Warehouse = s.SnowflakeWarehouse,
                            Database = s.SnowflakeDatabase,
                            Schema = s.SnowflakeSchema,
                            Account = Configuration.Config.GetHostSetting("SnowAccount"),
                            SnowflakeType = s.SnowflakeType
                        }).ToList());
            }

            //get the dataset's full list of permissions
            var permissions = datasetPermissions.Select(p =>
                new DatasetPermissionsUpdatedDto.PermissionDto()
                {
                    Identity = p.Identity,
                    IdentityType = p.IdentityType,
                    PermissionCode = p.SecurityPermission.Permission.PermissionCode,
                    Status = p.SecurityPermission.IsEnabled ? DatasetPermissionsUpdatedDto.PermissionDto.STATUS_ACTIVE : DatasetPermissionsUpdatedDto.PermissionDto.STATUS_REQUESTED
                }).ToList();

            //build up the changes specific to this ticket
            var changes = GetPermissionChanges(ticket, parentPermissions);

            //build the event details payload
            var details = new DatasetPermissionsUpdatedDto()
            {
                RequestId = ticket.SecurityTicketId.ToString(),
                DatasetId = dataset.DatasetId.ToString(),
                DatasetName = dataset.DatasetName,
                ApprovalTicket = ticket.TicketId,
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
        /// Build up a <see cref="DatasetPermissionsUpdatedDto.ChangesDto"/> based on the current <paramref name="ticket"/>
        /// </summary>
        /// <param name="ticket">The SecurityTicket that has just been requested or approved</param>
        /// <param name="parentActivePermissions">The permissions of the dataset's parent. These are needed because if inheritance was just removed, we need to publish that the parent's permissions no longer apply to the dataset.</param>
        internal static DatasetPermissionsUpdatedDto.ChangesDto GetPermissionChanges(SecurityTicket ticket, IList<SecurablePermission> parentActivePermissions)
        {
            var changes = new DatasetPermissionsUpdatedDto.ChangesDto()
            {
                Action = ticket.IsAddingPermission ? DatasetPermissionsUpdatedDto.ChangesDto.ACTION_ADD : DatasetPermissionsUpdatedDto.ChangesDto.ACTION_REMOVE,
                RequestedBy = ticket.RequestedById.ToString(),
                Permissions = new List<DatasetPermissionsUpdatedDto.PermissionDto>()
            };

            //if the permission that was just added (and approved!) is to inherit its parent, then publish the parent's permissions
            if (ticket.AddedPermissions.Any(p => p.Permission.PermissionCode == PermissionCodes.INHERIT_PARENT_PERMISSIONS) && ticket.ApprovedDate.HasValue)
            {
                //get the inherited permissions out of the fullListOfPermissions
                changes.Permissions.AddRange(
                    GetNotInheritedPermissionDtoList(parentActivePermissions, p => GetPermissionStatus(p.SecurityPermission)));
            }

            //if the permission that was just removed (and approved!) is the parent inheritance, then publish that the parent's permissions have been disabled
            if (ticket.RemovedPermissions.Any(p => p.Permission.PermissionCode == PermissionCodes.INHERIT_PARENT_PERMISSIONS) && ticket.ApprovedDate.HasValue)
            {
                //get the inherited permissions out of the fullListOfPermissions
                changes.Permissions.AddRange(
                    GetNotInheritedPermissionDtoList(parentActivePermissions, p => DatasetPermissionsUpdatedDto.PermissionDto.STATUS_DISABLED));
            }

            //if there are any added permissions (approved or just requested) other than the "inherit" permission
            if (ticket.AddedPermissions.Any(p => p.Permission.PermissionCode != PermissionCodes.INHERIT_PARENT_PERMISSIONS))
            {
                //get the permissions directly tied to this ticket
                changes.Permissions.AddRange(
                    GetNotInheritedPermissionDtoList(ticket.AddedPermissions, ticket));
            }

            //if there are any removed permissions (that have been approved!) other than the "inherit" permission
            if (ticket.RemovedPermissions.Any(p => p.Permission.PermissionCode != PermissionCodes.INHERIT_PARENT_PERMISSIONS) && ticket.ApprovedDate.HasValue)
            {
                changes.Permissions.AddRange(
                    GetNotInheritedPermissionDtoList(ticket.RemovedPermissions, ticket));
            }

            return changes;
        }

        private static List<DatasetPermissionsUpdatedDto.PermissionDto> GetNotInheritedPermissionDtoList(IList<SecurablePermission> permissions, Func<SecurablePermission, string> statusFunc)
        {
            return permissions.Where(p => p.SecurityPermission.Permission.PermissionCode != PermissionCodes.INHERIT_PARENT_PERMISSIONS).Select(p =>
                new DatasetPermissionsUpdatedDto.PermissionDto()
                {
                    Identity = p.Identity,
                    IdentityType = p.IdentityType,
                    PermissionCode = p.SecurityPermission.Permission.PermissionCode,
                    Status = statusFunc.Invoke(p)
                }).ToList();
        }

        private static List<DatasetPermissionsUpdatedDto.PermissionDto> GetNotInheritedPermissionDtoList(IList<SecurityPermission> permissions, SecurityTicket ticket)
        {
            return permissions.Where(p => p.Permission.PermissionCode != PermissionCodes.INHERIT_PARENT_PERMISSIONS).Select(p =>
                new DatasetPermissionsUpdatedDto.PermissionDto()
                {
                    Identity = ticket.Identity,
                    IdentityType = ticket.IdentityType,
                    PermissionCode = p.Permission.PermissionCode,
                    Status = GetPermissionStatus(p)
                }).ToList();
        }

        internal static string GetPermissionStatus(SecurityPermission securityPermission)
        {
            if (securityPermission.IsEnabled)
            {
                return DatasetPermissionsUpdatedDto.PermissionDto.STATUS_ACTIVE;
            }
            else if (securityPermission.RemovedDate.HasValue)
            {
                return DatasetPermissionsUpdatedDto.PermissionDto.STATUS_DISABLED;
            }
            else
            {
                return DatasetPermissionsUpdatedDto.PermissionDto.STATUS_REQUESTED;
            }
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
