using System;
using System.Linq;

namespace Sentry.data.Core
{
    public static class DatasetExtensions
    {
        public static string GenerateSASLibary(this DatasetDto dto, IDatasetContext dsContext)
        {
            return CommonExtensions.GenerateSASLibaryName(dsContext.GetById<Dataset>(dto.DatasetId));
        }

        public static DatasetTileDto ToDatasetTileDto(this Dataset dataset)
        {
            DatasetTileDto datasetTileDto = new DatasetTileDto();
            MapToDatasetTileDto(dataset, datasetTileDto);
            return datasetTileDto;
        }

        public static BusinessIntelligenceTileDto ToBusinessIntelligenceTileDto(this Dataset dataset)
        {
            BusinessIntelligenceTileDto biTileDto = new BusinessIntelligenceTileDto()
            {
                UpdateFrequency = Enum.GetName(typeof(ReportFrequency), dataset.Metadata?.ReportMetadata?.Frequency) ?? "Not Specified",
                ReportType = ((ReportType)dataset.DatasetFileConfigs.First().FileTypeId).ToString(),
                BusinessUnits = dataset.BusinessUnits.Select(x => x.Name).ToList(),
                Functions = dataset.DatasetFunctions.Select(x => x.Name).ToList(),
                Tags = dataset.Tags.Select(x => x.Name).ToList(),
            };

            MapToDatasetTileDto(dataset, biTileDto);
            return biTileDto;
        }

        private static void MapToDatasetTileDto(Dataset dataset, DatasetTileDto dto)
        {
            dto.Id = dataset.DatasetId.ToString();
            dto.Name = dataset.DatasetName;
            dto.Description = dataset.DatasetDesc;
            dto.Status = dataset.ObjectStatus;
            dto.IsSecured = dataset.IsSecured;
            dto.OriginationCode = dataset.OriginationCode;
            dto.Environment = dataset.NamedEnvironment;
            dto.EnvironmentType = dataset.NamedEnvironmentType.GetDescription();
            dto.Color = "darkgray";

            if (dataset.DatasetCategories?.Any() == true)
            {
                dto.Category = dataset.DatasetCategories.Select(x => x.Name).FirstOrDefault();
                dto.AbbreviatedCategory = string.Join(",", dataset.DatasetCategories.Select(x => x.AbbreviatedName ?? x.Name));

                if (dataset.DatasetCategories.Count == 1)
                {
                    dto.Color = dataset.DatasetCategories.First().Color;
                }
            }
        }
    }
}
