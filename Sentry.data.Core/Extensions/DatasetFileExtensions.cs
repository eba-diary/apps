using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public static class DatasetFileExtensions
    {
        public static List<string> ToObjectKeyVersion(this List<DatasetFileParquet> parquetFileList)
        {
            List<string> resultList = new List<string>();
            foreach (DatasetFileParquet file in parquetFileList)
            {
                resultList.Add(
                   $"parquet/{file.FileLocation}"
                );
            }
            return resultList;
        }


        public static IEnumerable<DatasetFileDto> ToDto(this IEnumerable<DatasetFile> datasetFileList)
        {
            List<DatasetFileDto> dtoList = new List<DatasetFileDto>();

            foreach (DatasetFile file in datasetFileList)
            {
                dtoList.Add(file.ToDto());
            }

            return dtoList.AsEnumerable();
        }

        public static DatasetFileDto ToDto(this DatasetFile file)
        {
            DatasetFileDto dto = new DatasetFileDto()
            {
                DatasetFileId = file.DatasetFileId,
                FileName = file.FileName,
                Dataset = file.Dataset.DatasetId,
                SchemaRevision = (file.SchemaRevision != null) ? file.SchemaRevision.SchemaRevision_Id : 0,
                Schema = file.Schema.SchemaId,
                DatasetFileConfig = (file.DatasetFileConfig != null) ? file.DatasetFileConfig.ConfigId : 0,
                UploadUserName = file.UploadUserName,
                CreateDTM = file.CreatedDTM,
                ModifiedDTM = file.ModifiedDTM,
                FileLocation = file.FileLocation,
                ParentDatasetFileId = file.ParentDatasetFileId,
                VersionId = file.VersionId,
                Information = file.Information,
                Size = file.Size,
                FlowExecutionGuid = file.FlowExecutionGuid,
                RunInstanceGuid = file.RunInstanceGuid,
                FileExtension = file.FileExtension,
                FileKey = file.FileKey,
                FileBucket = file.FileBucket,
                ETag = file.ETag,
                ObjectStatus = file.ObjectStatus,
            };

            return dto;
        }
    }
}
