using Sentry.data.Core;
using System.Linq;

namespace Sentry.data.Web
{
    public static class FilterSearchExtensions
    {
        public static DaleSearchDto ToDaleDto(this FilterSearchModel model)
        {
            return new DaleSearchDto()
            {
                Criteria = model.SearchText,
                FilterCategories = model.FilterCategories?.Select(x => x.ToDto()).ToList()
            };
        }
        
        public static SavedSearchDto ToDto(this SaveSearchModel model)
        {
            SavedSearchDto dto = new SavedSearchDto()
            {
                SavedSearchId = model.Id,
                SearchType = model.SearchType,
                SearchName = model.SearchName,
                AddToFavorites = model.AddToFavorites
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
                SearchText = dto.SearchText,
                FilterCategories = dto.FilterCategories?.Select(x => x.ToModel()).ToList()
            };
        }

        public static FilterCategoryModel ToModel(this FilterCategoryDto dto)
        {
            return new FilterCategoryModel()
            {
                CategoryName = dto.CategoryName,
                CategoryOptions = dto.CategoryOptions?.Select(x => x.ToModel()).ToList()
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
                IsFavorite = dto.IsFavorite
            };
        }

        private static void MapToParentDto(FilterSearchModel model, FilterSearchDto dto)
        {
            dto.SearchText = model.SearchText;
            dto.FilterCategories = model.FilterCategories?.Select(x => x.ToDto()).ToList();
        }
    }
}