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

        /// <summary>
        /// Gets a list of DatasetFileDrop Ids associated with the DatasetFile object
        /// </summary>
        /// <param name="file"></param>
        /// <param name="context"></param>
        /// <returns>
        /// May return 0 to many id's for a given DatasetFile object
        /// </returns>
        public static int GetDatasetFileDropIdListByDatasetFile(this DatasetFile file, IDatasetContext context)
        {
            //As files are reprocessed, there will be many DatasetFileQuery rows associated with single DatasetFileDrop row.
            //We are only pulling the DatasetFileDropId once, therefore, we are ordering and taking the top 1
            return context.DatasetFileQuery
                .Where(w => w.DatasetID == file.Dataset.DatasetId && w.SchemaId == file.Schema.SchemaId && w.FileNME == file.OriginalFileName)
                .OrderByDescending(o => o.DatasetFileQueryID)
                .Take(1)
                .Select(s => s.DatasetFileDrop)
                .FirstOrDefault();
        }
    }
}
