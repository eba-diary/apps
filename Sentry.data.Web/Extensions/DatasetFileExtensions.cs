using Sentry.data.Core;
using Sentry.data.Web.Models.ApiModels.DatasetFile;
using System;
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
                FileExtension = dto.FileExtension,
                FileKey = dto.FileKey,
                FileBucket = dto.FileBucket,
                ETag = dto.ETag,
                ObjectStatus = dto.ObjectStatus
            };

            return model;
        }
        
        public static DatasetFileDto ToDto(this DatasetFileModel model)
        {
            DatasetFileDto dto = new DatasetFileDto()
            {
                DatasetFileId = model.DatasetFileId,
                FileName = model.FileName,
                Dataset = model.DatasetId,
                SchemaRevision = model.SchemaRevisionId,
                Schema = model.SchemaId,
                DatasetFileConfig = model.DatasetFileConfigId,
                UploadUserName = model.UploadUserName,
                CreateDTM = DateTime.Parse(model.CreateDTM),
                ModifiedDTM = DateTime.Parse(model.CreateDTM),
                FileLocation = model.FileLocation,
                ParentDatasetFileId = model.ParentDatasetFileId,
                VersionId = model.VersionId,
                Information = model.Information,
                Size = model.Size,
                FlowExecutionGuid = model.FlowExecutionGuid,
                RunInstanceGuid = model.RunInstanceGuid,
                FileExtension = model.FileExtension,
                FileKey= model.FileKey,
                FileBucket= model.FileBucket,
                ETag= model.ETag,
                ObjectStatus = model.ObjectStatus
            };

            return dto;
        }

        public static UploadDatasetFileDto ToDto(this UploadDatasetFileModel model)
        {
            return new UploadDatasetFileDto()
            {
                DatasetId = model.DatasetId,
                ConfigId = model.ConfigId,
                FileName = model.DatasetFile.FileName,
                FileInputStream = model.DatasetFile.InputStream
            };
        }
    }
}