using Sentry.data.Core.GlobalEnums;
using System;

namespace Sentry.data.Core
{
    public class DatasetResultDto
    {
        public int DatasetId { get; set; }
        public string DatasetName { get; set; }
        public string CategoryName { get; set; }
        public string ShortName { get; set; }
        public string SaidAssetCode { get; set; }
        public string NamedEnvironment { get; set; }
        public NamedEnvironmentType NamedEnvironmentType { get; set; }
        public string DatasetDescription { get; set; }
        public string UsageInformation { get; set; }
        public DataClassificationType DataClassificationType { get; set; }
        public bool IsSecured { get; set; }
        public string PrimaryContactId { get; set; }
        public string AlternateContactEmail { get; set; }
        public DatasetOriginationCode OriginationCode { get; set; }
        public string OriginalCreator { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime UpdatedDateTime { get; set; }
        public ObjectStatusEnum ObjectStatus { get; set; }
    }
}
