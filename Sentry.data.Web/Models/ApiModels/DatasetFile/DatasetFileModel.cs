using System;
using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;

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
        public string FileKey { get; set; }
        public string FileBucket { get; set; }
        public string ETag { get; set; }
        public string ObjectStatus { get; set; }

        public List<string> Validate()
        {
            List<string> results = new List<string>();
            if (DatasetFileId == 0)
            {
                results.Add("DatasetFileId is required");
            }
            if (string.IsNullOrWhiteSpace(FileName))
            {
                results.Add("FileName is required");
            }
            if (DatasetId == 0)
            {
                results.Add("DatasetId is required");
            }
            if (SchemaId == 0)
            {
                results.Add("SchemaId is required");
            }

            //if (string.Equals(Format, "csv", StringComparison.OrdinalIgnoreCase) && Delimiter != ",")
            //{
            //    results.Add("File Extension CSV and it's delimiter do not match");
            //}

            return results;
        }
    }
}