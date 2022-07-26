using Sentry.data.Core;
using Sentry.data.Web.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Web
{
    public static class TileExtensions
    {
        public static TileResultsModel ToModel(this DatasetSearchResultDto dto, int selectedSortByValue, int selectedPageNumber)
        {
            TileResultsModel model = new TileResultsModel()
            {
                TotalResults = dto.TotalResults,
                Tiles = dto.Tiles.ToModel(),
                FilterCategories = dto.FilterCategories.ToModel(),
                PageSizeOptions = Utility.BuildTilePageSizeOptions(dto.PageSize.ToString()),
                SortByOptions = Utility.BuildDatasetSortByOptions(selectedSortByValue),
                PageItems = Utility.BuildPageItemList(dto.TotalResults, dto.PageSize, selectedPageNumber)
            };

            return model;
        }

        private static List<TileModel> ToModel(this List<DatasetTileDto> dtos)
        {
            return dtos.Select(x => x.ToModel()).ToList();
        }

        private static TileModel ToModel(this DatasetTileDto dto)
        {
            return new TileModel()
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                Status = dto.Status.GetDescription(),
                TileTitle = dto.Status == Core.GlobalEnums.ObjectStatusEnum.Active ? "Click here to go to the Dataset Detail Page" : "Dataset is marked for deletion",
                FavoriteTitle = dto.Status == Core.GlobalEnums.ObjectStatusEnum.Active ? "Click to toggle favorite" : "Dataset is marked for deletion, favorite functionality disabled",
                IsFavorite = dto.IsFavorite,
                Category = dto.Category,
                Color = dto.Color,
                IsSecured = dto.IsSecured,
                LastUpdated = dto.LastUpdated.ToShortDateString(),
                IsReport = false,
                ReportTypes = new List<string>()
            };
        }
    }
}