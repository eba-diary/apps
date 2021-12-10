namespace Sentry.data.Web.Models.ApiModels.DatasetFile
{
    public class DatasetFileModel
    {
        public int DatasetFileId { get; set; }

        public string FileName { get; set; }

        public int DatasetId { get; set; }
        public int SchemaRevisionId { get; set; }

        public int SchemaId { get; set; }

        public int DatasetFileConfigId { get; set; }

        public string UploadUserName { get; set; }

        public string CreateDTM { get; set; }

        public string ModifiedDTM { get; set; }

        public string FileLocation { get; set; }

        public int ParentDatasetFileId { get; set; }

        public string VersionId { get; set; }

        public string Information { get; set; }

        public long Size { get; set; }

        public string FlowExecutionGuid { get; set; }

        public string RunInstanceGuid { get; set; }
        public string FileExtension { get; set; }
    }
}