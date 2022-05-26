using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Interfaces.InfrastructureEventing;
using Sentry.data.Infrastructure.InfrastructureEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    /// <summary>
    /// Infrastructure Eventing Service - responsible for interacting with the REST Inev endpoint to publish/consume messages
    /// </summary>
    public class InevService : IInevService
    {
        private readonly IClient _inevRestClient;

        /// <summary>
        /// Public constructor
        /// </summary>
        public InevService(IClient inevRestClient)
        {
            _inevRestClient = inevRestClient;
        }

        /// <summary>
        /// Publish a "DatasetPermissionsUpdated" event to Infrastructure Eventing
        /// </summary>
        /// <param name="dataset">The dataset whose permissions were updated</param>
        /// <param name="ticket">The ticket that was just approved</param>
        /// <param name="securablePermissions">The full list of securable permissions on this dataset</param>
        public async Task PublishDatasetPermissionsUpdated(Dataset dataset, SecurityTicket ticket, IList<SecurablePermission> securablePermissions)
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
                });
            }

            //get the dataset's full list of permissions
            var permissions = securablePermissions.Select(p =>
                new DatasetPermissionsUpdatedDto.PermissionDto()
                {
                    Identity = p.Identity,
                    IdentityType = p.IdentityType,
                    PermissionCode = p.SecurityPermission.Permission.PermissionCode,
                    Status = p.SecurityPermission.IsEnabled ? "active" : "requested"
                }).ToList();

            //build up the changes specific to this ticket that was just approved
            var changes = new DatasetPermissionsUpdatedDto.ChangesDto()
            {
                Action = ticket.IsAddingPermission ? "add" : "remove",
                RequestedBy = ticket.RequestedById.ToString(),
                Permissions = ticket.Permissions.Select(p =>
                    new DatasetPermissionsUpdatedDto.PermissionDto()
                    {
                        Identity = ticket.Identity,
                        IdentityType = ticket.IdentityType,
                        PermissionCode = p.Permission.PermissionCode,
                        Status = p.IsEnabled ? "active" : "requested"
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

            //publish the Infrastructure Event
            await _inevRestClient.PublishUsingPOSTAsync(
                    new Message()
                    {
                        Topic = "INEV-DataLake",
                        MessageSource = "DSC",
                        SaidKey = "DATA",
                        EventType = "DatasetPermissionsUpdated",
                        Details = details.ToDictionary()
                    }
                );
        }
    }
}
