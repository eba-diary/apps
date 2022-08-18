using Newtonsoft.Json.Linq;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Web
{
    public static class FilterSearchExtensions
    {
        public static FilterSearchDto ToDto(this FilterSearchModel model)
        {
            FilterSearchDto dto = new FilterSearchDto();
            MapToParentDto(model, dto);
            return dto;
        }
        
        public static SavedSearchDto ToDto(this SaveSearchModel model)
        {
            SavedSearchDto dto = new SavedSearchDto()
            {
                SavedSearchId = model.Id,
                SearchType = model.SearchType,
                AddToFavorites = model.AddToFavorites,
                ResultConfiguration = !string.IsNullOrEmpty(model.ResultConfigurationJson) ? JObject.Parse(model.ResultConfigurationJson) : null
            };

            MapToParentDto(model, dto);

            return dto;
        }

        public static FilterCategoryDto ToDto(this FilterCategoryModel model)
        {
            return new FilterCategoryDto()
            {
                CategoryName = model.CategoryName,
                CategoryOptions = model.CategoryOptions?.Select(x => x.ToDto()).ToList()
            };
        }

        public static FilterCategoryOptionDto ToDto(this FilterCategoryOptionModel model)
        {
            return new FilterCategoryOptionDto() 
            {
                OptionValue = FilterCategoryOptionNormalizer.Denormalize(model.ParentCategoryName, model.OptionValue),
                ResultCount = model.ResultCount,
                ParentCategoryName = model.ParentCategoryName,
                Selected = model.Selected
            };
        }

        public static FilterSearchModel ToModel(this FilterSearchDto dto)
        {
            return new FilterSearchModel()
            {
                SearchName = dto.SearchName,
                SearchText = dto.SearchText,
                FilterCategories = dto.FilterCategories.ToModels()
            };
        }

        public static List<FilterCategoryModel> ToModels(this List<FilterCategoryDto> dtos)
        {
            return dtos?.Select(x => x.ToModel()).ToList();
        }

        private static FilterCategoryModel ToModel(this FilterCategoryDto dto)
        {
            return new FilterCategoryModel()
            {
                CategoryName = dto.CategoryName,
                CategoryOptions = dto.CategoryOptions?.Select(x => x.ToModel()).ToList(),
                DefaultCategoryOpen = dto.DefaultCategoryOpen,
                HideResultCounts = dto.HideResultCounts
            };
        }

        public static FilterCategoryOptionModel ToModel(this FilterCategoryOptionDto dto)
        {
            return new FilterCategoryOptionModel()
            {
                OptionValue = FilterCategoryOptionNormalizer.Normalize(dto.ParentCategoryName, dto.OptionValue),
                ResultCount = dto.ResultCount,
                ParentCategoryName = dto.ParentCategoryName,
                Selected = dto.Selected
            };
        }

        public static SavedSearchOptionModel ToModel(this SavedSearchOptionDto dto)
        {
            return new SavedSearchOptionModel
            {
                SavedSearchId = dto.SavedSearchId,
                SavedSearchName = dto.SavedSearchName,
                SavedSearchUrl = dto.SavedSearchUrl,
                IsFavorite = dto.IsFavorite
            };
        }

        public static TileSearchDto<T> ToDto<T>(this TileSearchModel model) where T : DatasetTileDto
        {
            TileSearchDto<T> dto = new TileSearchDto<T>()
            {
                PageSize = model.PageSize,
                PageNumber = model.PageNumber,
                UpdateFilters = model.UpdateFilters
            };

            switch ((TileSearchSortByOption)model.SortBy)
            {
                case TileSearchSortByOption.Alphabetical:
                    dto.OrderByField = x => x.Name;
                    break;
                case TileSearchSortByOption.Favorites:
                    dto.OrderByField = x => x.IsFavorite;
                    dto.OrderByDescending = true;
                    break;
                case TileSearchSortByOption.RecentlyAdded:
                    dto.OrderByField = x => x.CreatedDateTime;
                    dto.OrderByDescending = true;
                    break;
                case TileSearchSortByOption.RecentlyUpdated:
                    dto.OrderByField = x => x.LastActivityDateTime;
                    dto.OrderByDescending = true;
                    break;
                default:
                    break;
            }

            MapToParentDto(model, dto);

            return dto;
        }

        public static TileSearchEventDto ToEventDto(this TileSearchModel model, int totalResults)
        {
            TileSearchEventDto dto = new TileSearchEventDto()
            {
                PageNumber = model.PageNumber,
                PageSize = model.PageSize,
                SortBy = model.SortBy,
                Layout = model.Layout,
                TotalResults = totalResults
            };

            MapToParentDto(model, dto);
            return dto;
        }

        public static List<DatasetTileDto> ToDatasetTileDtos(this List<TileModel> models)
        {
            List<DatasetTileDto> tileDtos = new List<DatasetTileDto>();

            if (models?.Any() == true)
            {
                foreach (TileModel model in models)
                {
                    DatasetTileDto dto = new DatasetTileDto();
                    MapToParentDto(model, dto);
                    tileDtos.Add(dto);
                }
            }

            return tileDtos;
        }

        public static List<BusinessIntelligenceTileDto> ToBusinessIntelligenceTileDtos(this List<TileModel> models)
        {
            List<BusinessIntelligenceTileDto> tileDtos = new List<BusinessIntelligenceTileDto>();

            if (models?.Any() == true)
            {
                foreach (TileModel model in models)
                {
                    BusinessIntelligenceTileDto dto = new BusinessIntelligenceTileDto()
                    {
                        AbbreviatedCategories = model.AbbreviatedCategories,
                        ReportType = model.ReportType,
                        UpdateFrequency = model.UpdateFrequency,
                        ContactNames = model.ContactNames,
                        BusinessUnits = model.BusinessUnits,
                        Functions = model.Functions,
                        Tags = model.Tags
                    };

                    MapToParentDto(model, dto);
                    tileDtos.Add(dto);
                }
            }

            return tileDtos;
        }

        private static void MapToParentDto(TileModel model, DatasetTileDto dto)
        {
            dto.Id = model.Id;
            dto.Name = model.Name;
            dto.Description = model.Description;
            dto.Status = EnumHelper.GetByDescription<ObjectStatusEnum>(model.Status);
            dto.IsFavorite = model.IsFavorite;
            dto.Category = model.Category;
            dto.Color = model.Color;
            dto.IsSecured = model.IsSecured;
            dto.LastActivityDateTime = DateTime.Parse(model.LastActivityDateTime);
            dto.CreatedDateTime = DateTime.Parse(model.CreatedDateTime);
        }

        private static void MapToParentDto(FilterSearchModel model, FilterSearchDto dto)
{
            dto.SearchName = model.SearchName;
            dto.SearchText = model.SearchText;
            dto.FilterCategories = model.FilterCategories?.Select(x => x.ToDto()).ToList();
        }
    }
}