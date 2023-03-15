using Sentry.data.Core;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sentry.data.Infrastructure.InfrastructureEvents
{
    /// <summary>
    /// This DTO is used to create the details of the DatasetPermissionsUpdated Infrastructure Event
    /// </summary>
    public class DatasetPermissionsUpdatedDto
    {
        /// <summary>
        /// The Security Ticket ID that just got approved that is causing this Infrastructure Event to be generated
        /// </summary>
        public string RequestId { get; set; }
        /// <summary>
        /// The Approval Ticket ID that DSC obtained to make this change
        /// </summary>
        public string ApprovalTicket { get; set; }
        /// <summary>
        /// The Support Email for permission issues
        /// </summary>
        public string SupportEmail { get; set; }
        /// <summary>
        /// The Dataset ID
        /// </summary>
        public string DatasetId { get; set; }
        /// <summary>
        /// The Dataset Name
        /// </summary>
        public string DatasetName { get; set; }
        /// <summary>
        /// The SAID asset key assigned to this dataset
        /// </summary>
        public string DatasetSaidKey { get; set; }
        /// <summary>
        /// The Quartermaster Named Environment that this dataset is from
        /// </summary>
        public string DatasetNamedEnvironment { get; set; }
        /// <summary>
        /// The type of Quartermaster Named Environment that this dataset is from (PROD or NONPROD)
        /// </summary>
        public string DatasetNamedEnvironmentType { get; set; }
        /// <summary>
        /// Information about how this dataset is represented in Snowflake
        /// This is a list to support the case where the dataset appears in multiple places in Snowflake
        /// </summary>
        public IList<SnowflakeDto> Snowflake { get; set; }
        /// <summary>
        /// The full list of permissions that this dataset has
        /// </summary>
        public IList<PermissionDto> Permissions { get; set; }
        /// <summary>
        /// Specifics about the change that triggered this DatasetPermissionsUpdated Infrastructure Event
        /// </summary>
        public ChangesDto Changes { get; set; }

        /// <summary>
        /// Contains Snowflake-specific dataset information
        /// </summary>
        public class SnowflakeDto
        {
            /// <summary>
            /// The Snowflake account - will always be sentry.us-east-2.aws
            /// </summary>
            public string Account { get; set; }
            /// <summary>
            /// The Snowflake Warehouse - for example, DATA_WH
            /// </summary>
            public string Warehouse { get; set; }
            /// <summary>
            /// The Snowflake Database - for example, DATA_PROD or WDAY_PROD
            /// </summary>
            public string Database { get; set; }
            /// <summary>
            /// The Snowflake Schema - for example, CLAIM or ZZZDataset
            /// </summary>
            public string Schema { get; set; }
            /// <summary>
            /// What type of Snowflake instance this is
            /// </summary>
            public SnowflakeConsumptionType SnowflakeType { get; set; }
        }

        public class PermissionDto
        {
            public const string STATUS_ACTIVE = "active";
            public const string STATUS_REQUESTED = "requested";
            public const string STATUS_DISABLED = "disabled";

            public string Identity { get; set; }
            public string IdentityType { get; set; }
            public string PermissionCode { get; set; }
            public string Status { get; set; }
        }

        public class ChangesDto
        {
            public const string ACTION_ADD = "add";
            public const string ACTION_REMOVE = "remove";

            public string Action { get; set; }
            public string RequestedBy { get; set; }
            public List<PermissionDto> Permissions { get; set; }
        }

        public Dictionary<string, string> ToDictionary()
        {
            var serializerOptions = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, Converters = { new JsonStringEnumConverter() } };
            return new Dictionary<string, string>
            {
                { nameof(RequestId).ToLowerFirstChar(), RequestId },
                { nameof(DatasetId).ToLowerFirstChar(), DatasetId },
                { nameof(DatasetName).ToLowerFirstChar(), DatasetName },
                { nameof(DatasetSaidKey).ToLowerFirstChar(), DatasetSaidKey },
                { nameof(DatasetNamedEnvironment).ToLowerFirstChar(), DatasetNamedEnvironment },
                { nameof(DatasetNamedEnvironmentType).ToLowerFirstChar(), DatasetNamedEnvironmentType },
                { nameof(Snowflake).ToLowerFirstChar(), JsonSerializer.Serialize(Snowflake, serializerOptions) },
                { nameof(Permissions).ToLowerFirstChar(), JsonSerializer.Serialize(Permissions, serializerOptions) },
                { nameof(Changes).ToLowerFirstChar(), JsonSerializer.Serialize(Changes, serializerOptions) }
            };
        }
    }

}
