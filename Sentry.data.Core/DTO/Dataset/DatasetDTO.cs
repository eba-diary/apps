using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class DatasetDto : BaseEntityDto
    {
        public int OriginationId { get; set; }
        public string DatasetInformation { get; set; }
        public string CategoryName { get; set; }
        public DataClassificationType DataClassification { get; set; }
        public string SAIDAssetKeyCode { get; set; }
        public string NamedEnvironment { get; set; }
        public NamedEnvironmentType NamedEnvironmentType { get; set; }
        public string ShortName { get; set; }
        public int? GlobalDatasetId { get; set; }
        public string SnowflakeWarehouse { get; set; }
        public List<string> SnowflakeDatabases { get; set; }
        public string SnowflakeSchema { get; set; }
    }
}
