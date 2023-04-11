namespace Sentry.data.Core
{
    public class DatasetSchemaDto : DatasetDto
    {
        public string ConfigFileName { get; set; }
        public string ConfigFileDesc { get; set; }
        public int FileExtensionId { get; set; }
        public string Delimiter { get; set; }
        public string SchemaRootPath { get; set; }
        public bool HasHeader { get; set; }
        public int DatasetScopeTypeId { get; set; }
        public bool CreateCurrentView { get; set; }
        public int SchemaId { get; set; }
    }
}
