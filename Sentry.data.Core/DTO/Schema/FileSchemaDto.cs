namespace Sentry.data.Core
{
    public class FileSchemaDto : SchemaDto
    {
        public override string SchemaEntity_NME { get; set; }
        public int FileExtensionId { get; set; }
        public string FileExtenstionName { get; set; }
        public string Delimiter { get; set; }
        public bool HasHeader { get; set; }
        public bool CreateCurrentView { get; set; }
        public string SasLibrary { get; set; }
        public string HiveTable { get; set; }
        public string HiveDatabase { get; set; }
        public string HiveLocation { get; set; }
        public string HiveStatus { get; set; }
        public string StorageCode { get; set; }
        public string StorageLocation { get; set; }

        public string RawQueryStorage { get; set; }
        public string SnowflakeDatabase { get; set; }
        public string SnowflakeTable { get; set; }
        public string SnowflakeSchema { get; set; }
        public string SnowflakeStatus { get; set; }
        public string SchemaRootPath { get; set; }
        public string ParquetStorageBucket { get; set; }
        public string ParquetStoragePrefix { get; set; }
        public string SnowflakeStage { get; set; }
        public string SnowflakeWarehouse { get; set; }
    }
}
