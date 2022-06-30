using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public static class DatasetFileConfigExtensions
    {
        public static string GenerateSASLibaryName(this DatasetFileConfigDto dto, IDatasetContext dsContext)
        {
            return CommonExtensions.GenerateSASLibaryName(dsContext.GetById<Dataset>(dto.ParentDatasetId));
        }

        public static DatasetFileConfigDto ToConfigDto(this DatasetDto dsDto)
        {
            List<DataElementDto> delist = new List<DataElementDto>();
            delist.Add(dsDto.ToDataElementDto());

            DatasetFileConfigDto dto = new DatasetFileConfigDto()
            {
                Name = dsDto.ConfigFileName,
                Description = dsDto.ConfigFileDesc,
                FileTypeId = (int)Core.FileType.DataFile,
                ParentDatasetId = dsDto.DatasetId,
                DatasetScopeTypeId = dsDto.DatasetScopeTypeId,
                FileExtensionId = dsDto.FileExtensionId,
                Delimiter = dsDto.Delimiter,
                HasHeader = dsDto.HasHeader,
                CreateCurrentView = dsDto.CreateCurrentView,
                ObjectStatus = dsDto.ObjectStatus,
                SchemaRootPath = dsDto.SchemaRootPath
            };
            dto.Schemas = delist;
            return dto;
        }

        public static FileSchemaDto ToSchemaDto (this DatasetDto dsDto)
        {
            return new FileSchemaDto()
            {
                Name = dsDto.ConfigFileName,
                Delimiter = dsDto.Delimiter,
                FileExtensionId = dsDto.FileExtensionId,
                HasHeader = dsDto.HasHeader,
                Description = dsDto.ConfigFileDesc,
                ParentDatasetId = dsDto.DatasetId,
                CreateCurrentView = dsDto.CreateCurrentView,
                ObjectStatus = dsDto.ObjectStatus,
                SchemaRootPath = dsDto.SchemaRootPath
            };
        }

        public static DatasetFileConfigSchemaDto ToDatasetFileConfigSchemaDto(this DatasetFileConfig datasetFileConfig)
        {
            return new DatasetFileConfigSchemaDto()
            {
                ConfigId = datasetFileConfig.ConfigId,
                SchemaId = datasetFileConfig.Schema.SchemaId,
                SchemaName = datasetFileConfig.Name
            };
        }
    }
}
