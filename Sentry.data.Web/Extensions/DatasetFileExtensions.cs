using Sentry.data.Core;
using Sentry.data.Web.Models.ApiModels.DatasetFile;
using System.Collections.Generic;

namespace Sentry.data.Web
{
    public static class DatasetFileExtensions
    {
        public static List<DatasetFileModel> ToModel(this List<DatasetFileDto> dtoList)
        {
            List<DatasetFileModel> modelList = new List<DatasetFileModel>();
            foreach (DatasetFileDto dto in dtoList)
            {
                modelList.Add(dto.ToModel());
            }

            return modelList;
        }

        public static DatasetFileModel ToModel(this DatasetFileDto dto)
        {
            DatasetFileModel model = new DatasetFileModel()
            {
                DatasetFileId = dto.DatasetFileId,
                FileName = dto.FileName,
                DatasetId = dto.Dataset,
                SchemaRevisionId = dto.SchemaRevision,
                SchemaId = dto.Schema,
                DatasetFileConfigId = dto.DatasetFileConfig,
                UploadUserName = dto.UploadUserName,
                CreateDTM = dto.CreateDTM.ToString("s"),
                ModifiedDTM = dto.CreateDTM.ToString("s"),
                FileLocation = dto.FileLocation,
                ParentDatasetFileId = dto.ParentDatasetFileId ?? 0,
                VersionId = dto.VersionId,
                Information = dto.Information,
                Size = dto.Size,
                FlowExecutionGuid = dto.FlowExecutionGuid,
                RunInstanceGuid = dto.RunInstanceGuid,
                FileExtension = dto.FileExtension
            };

            return model;
        } 
    }
}