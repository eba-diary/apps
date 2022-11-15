using Sentry.data.Core.GlobalEnums;

namespace Sentry.data.Core
{
    public class DatasetDtoV2 : BaseEntityDto
    {
        public int OriginationId { get; set; }
        public string DatasetInformation { get; set; }
        public string CategoryName { get; set; }
        public DataClassificationType DataClassification { get; set; }
        public string SAIDAssetKeyCode { get; set; }
        public string NamedEnvironment { get; set; }
        public NamedEnvironmentType NamedEnvironmentType { get; set; }
        public string ShortName { get; set; }
    }
}
