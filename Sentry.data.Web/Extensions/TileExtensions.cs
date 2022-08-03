using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Web.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Web
{
    public static class TileExtensions
    {
        public static TileResultsModel ToModel(this DatasetSearchResultDto dto, int selectedSortByValue, int selectedPageNumber, int selectedLayout)
        {
            TileResultsModel model = new TileResultsModel()
            {
                TotalResults = dto.TotalResults,
                Tiles = dto.Tiles.ToModels(),
                PageSizeOptions = Utility.BuildTilePageSizeOptions(dto.PageSize.ToString()),
                SortByOptions = Utility.BuildSelectListFromEnum<DatasetSortByOption>(selectedSortByValue).Where(x => x.Value != ((int)DatasetSortByOption.MostAccessed).ToString()).ToList(),
                PageItems = Utility.BuildPageItemList(dto.TotalResults, dto.PageSize, selectedPageNumber),
                LayoutOptions = Utility.BuildSelectListFromEnum<LayoutOption>(selectedLayout)
            };

            return model;
        }

        public static List<TileModel> ToModels(this List<DatasetTileDto> dtos)
        {
            return dtos?.Select(x => x.ToModel()).ToList();
        }

        private static TileModel ToModel(this DatasetTileDto dto)
        {
            return new TileModel()
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                Status = dto.Status.GetDescription(),
                TileTitle = dto.Status == ObjectStatusEnum.Active ? "Click here to go to the Dataset Detail Page" : "Dataset is marked for deletion",
                FavoriteTitle = dto.Status == ObjectStatusEnum.Active ? "Click to toggle favorite" : "Dataset is marked for deletion, favorite functionality disabled",
                IsFavorite = dto.IsFavorite,
                Category = dto.Category,
                Color = dto.Color,
                IsSecured = dto.IsSecured,
                LastActivityDateTime = dto.LastActivityDateTime.ToShortDateString(),
                CreatedDateTime = dto.CreatedDateTime.ToShortDateString(),
                IsReport = false,
                ReportTypes = new List<string>()
            };
        }
    }
}