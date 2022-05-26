using System.Collections.Generic;
using System.Text.Json;

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
        }

        public class PermissionDto
        {
            public string Identity { get; set; }
            public string IdentityType { get; set; }
            public string PermissionCode { get; set; }
            public string Status { get; set; }
        }

        public class ChangesDto
        {
            public string Action { get; set; }
            public string RequestedBy { get; set; }
            public IList<PermissionDto> Permissions { get; set; }
        }

        public Dictionary<string, string> ToDictionary()
        {
            var dict = new Dictionary<string, string>();
            dict.Add(nameof(RequestId), RequestId);
            dict.Add(nameof(DatasetId), DatasetId);
            dict.Add(nameof(DatasetName), DatasetName);
            dict.Add(nameof(DatasetSaidKey), DatasetSaidKey);
            dict.Add(nameof(DatasetNamedEnvironment), DatasetNamedEnvironment);
            dict.Add(nameof(DatasetNamedEnvironmentType), DatasetNamedEnvironmentType);
            dict.Add(nameof(Snowflake), JsonSerializer.Serialize(Snowflake));
            dict.Add(nameof(Permissions), JsonSerializer.Serialize(Permissions));
            dict.Add(nameof(Changes), JsonSerializer.Serialize(Changes));
            return dict;
        }
    }

}
