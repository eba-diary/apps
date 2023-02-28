using Sentry.data.Core.GlobalEnums;

namespace Sentry.data.Core
{
    public class SchemaResultDto : BaseResultDto
    {
        public int SchemaId { get; set; }
        public string SchemaDescription { get; set; }
        public string Delimiter { get; set; }
        public bool HasHeader { get; set; }
        public string ScopeTypeCode { get; set; }
        public string FileTypeCode { get; set; }
        public string SchemaRootPath { get; set; }
        public bool CreateCurrentView { get; set; }
        public IngestionType IngestionType { get; set; }
        public bool IsCompressed { get; set; }
        public string CompressionTypeCode { get; set; }
        public bool IsPreprocessingRequired { get; set; }
        public string PreprocessingTypeCode { get; set; }
        public int DatasetId { get; set; }
        public string SchemaName { get; set; }
        public string SaidAssetCode { get; set; }
        public string NamedEnvironment { get; set; }
        public NamedEnvironmentType NamedEnvironmentType { get; set; }
        public string KafkaTopicName { get; set; }
        public string PrimaryContactId { get; set; }
        public string StorageCode { get; set; }
        public string DropLocation { get; set; }
        public string ControlMTriggerName { get; set; }
    }
}
