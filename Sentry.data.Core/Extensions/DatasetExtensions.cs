using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public static class DatasetExtensions
    {
        public static string GenerateSASLibary(this DatasetDto dto, IDatasetContext dsContext)
        {
            return CommonExtensions.GenerateSASLibaryName(dsContext.GetById<Dataset>(dto.DatasetId));
        }

        public static DatasetTileDto ToTileDto(this Dataset dataset)
        {
            DatasetTileDto datasetTileDto = new DatasetTileDto()
            {
                Id = dataset.DatasetId.ToString(),
                Name = dataset.DatasetName,
                Description = dataset.DatasetDesc,
                Status = dataset.ObjectStatus,
                IsSecured = dataset.IsSecured
            };

            if (dataset.DatasetCategories?.Any() == true)
            {
                List<string> catNameList = dataset.DatasetCategories.Select(x => !string.IsNullOrWhiteSpace(x.AbbreviatedName) ? x.AbbreviatedName : x.Name).ToList();
                datasetTileDto.Category = string.Join(", ", catNameList);
            }

            return datasetTileDto;
        }
    }
}
