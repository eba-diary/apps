using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Web.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Web
{
    public static class TileExtensions
    {
        public static TileResultsModel ToModel<T>(this TileSearchResultDto<T> dto, int selectedSortByValue, int  selectedLayout)
        {
            TileResultsModel model = new TileResultsModel()
            {
                TotalResults = dto.TotalResults,
                PageSizeOptions = Utility.BuildTilePageSizeOptions(dto.PageSize.ToString()),
                SortByOptions = Utility.BuildSelectListFromEnum<TileSearchSortByOption>(selectedSortByValue),
                PageItems = Utility.BuildPageItemList(dto.TotalResults, dto.PageSize, dto.PageNumber),
                LayoutOptions = Utility.BuildSelectListFromEnum<LayoutOption>(selectedLayout),
                FilterCategories = dto.FilterCategories.ToModels()
            };

            return model;
        }

        public static List<TileModel> ToModels(this List<DatasetTileDto> dtos)
        {
            List<TileModel> tileModels = new List<TileModel>();

            if (dtos?.Any() == true)
            {
                foreach (DatasetTileDto dto in dtos)
                {
                    TileModel model = new TileModel();
                    MapToParentModel(dto, model);
                    tileModels.Add(model);
                }
            }

            return tileModels;
        }

        public static List<TileModel> ToModels(this List<BusinessIntelligenceTileDto> dtos)
        {
            List<TileModel> tileModels = new List<TileModel>();

            if (dtos?.Any() == true)
            {
                foreach (BusinessIntelligenceTileDto dto in dtos)
                {
                    TileModel model = new TileModel()
                    {
                        AbbreviatedCategories = dto.AbbreviatedCategories, 
                        IsReport = true,
                        ReportType = dto.ReportType,
                        UpdateFrequency = dto.UpdateFrequency,
                        ContactNames = dto.ContactNames,
                        BusinessUnits = dto.BusinessUnits,
                        Tags = dto.Tags
                    };

                    MapToParentModel(dto, model);
                    tileModels.Add(model);
                }
            }

            return tileModels;
        }

        private static void MapToParentModel(DatasetTileDto dto, TileModel model)
        {
            model.Id = dto.Id;
            model.Name = dto.Name;
            model.Description = dto.Description;
            model.Status = dto.Status.GetDescription();
            model.TileTitle = dto.Status == ObjectStatusEnum.Active ? "Click here to go to the Dataset Detail Page" : "Dataset is marked for deletion";
            model.FavoriteTitle = dto.Status == ObjectStatusEnum.Active ? "Click to toggle favorite" : "Dataset is marked for deletion; favorite functionality disabled";
            model.IsFavorite = dto.IsFavorite;
            model.Category = dto.Category;
            model.Color = dto.Color;
            model.IsSecured = dto.IsSecured;
            model.LastActivityDateTime = dto.LastActivityDateTime.ToShortDateString();
            model.CreatedDateTime = dto.CreatedDateTime.ToShortDateString();
            model.IsReport = false;
        }
    }
}