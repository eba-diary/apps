using Sentry.data.Core;
using System.Linq;

namespace Sentry.data.Web
{
    public static class FilterSearchExtensions
    {
        public static DaleSearchDto ToDto(this FilterSearchModel model)
        {
            return new DaleSearchDto()
            {
                Criteria = model.SearchText,
                Filters = model.FilterCategories?.Select(x => x.ToDto()).ToList()
            };
        }

        public static FilterCategoryDto ToDto(this FilterCategoryModel model)
        {
            return new FilterCategoryDto()
            {
                CategoryName = model.CategoryName,
                CategoryOptions = model.CategoryOptions?.Where(x => x.Selected).Select(x => x.ToDto()).ToList()
            };
        }

        public static FilterCategoryOptionDto ToDto(this FilterCategoryOptionModel model)
        {
            return new FilterCategoryOptionDto() 
            {
                OptionValue = model.OptionValue,
                ResultCount = model.ResultCount,
                ParentCategoryName = model.ParentCategoryName,
                Selected = model.Selected
            };
        }

        public static FilterSearchModel ToModel(this DaleSearchDto dto)
        {
            return new FilterSearchModel()
            {
                SearchText = dto.Criteria,
                FilterCategories = dto.Filters?.Select(x => x.ToModel()).ToList()
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
                OptionValue = dto.OptionValue,
                ResultCount = dto.ResultCount,
                ParentCategoryName = dto.ParentCategoryName,
                Selected = dto.Selected
            };
        }
    }
}