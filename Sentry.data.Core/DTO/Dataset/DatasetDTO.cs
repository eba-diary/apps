using Sentry.data.Core.GlobalEnums;

namespace Sentry.data.Core
{
    public class DatasetDto : BaseEntityDto
    {

        public int OriginationId { get; set; }
        public string ConfigFileName { get; set; }
        public string ConfigFileDesc { get; set; }
        public int FileExtensionId { get; set; }
        public string Delimiter { get; set; }
        public string SchemaRootPath { get; set; }
        public bool HasHeader { get; set; }
        public int DatasetScopeTypeId { get; set; }
        public string DatasetInformation { get; set; }
        public string CategoryName { get; set; }
        public DataClassificationType DataClassification { get; set; }
        public bool CreateCurrentView { get; set; }
        public string SAIDAssetKeyCode { get; set; }
        public string NamedEnvironment { get; set; }
        public NamedEnvironmentType NamedEnvironmentType { get; set; }
        public string ShortName { get; set; }
    }
}
