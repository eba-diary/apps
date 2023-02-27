namespace Sentry.data.Web.API
{
    public abstract class BaseSchemaModel
    {
        public string SchemaDescription { get; set; }
        public string Delimiter { get; set; }
        public bool HasHeader { get; set; }
        public string ScopeTypeCode { get; set; }
        public string FileTypeCode { get; set; }
        public string SchemaRootPath { get; set; }
        public bool CreateCurrentView { get; set; }
        public string IngestionTypeCode { get; set; }
        public bool IsCompressed { get; set; }
        public string CompressionTypeCode { get; set; }
        public bool IsPreprocessingRequired { get; set; }
        public string PreprocessingCode { get; set; }
        public string KafkaTopicName { get; set; }
    }
}