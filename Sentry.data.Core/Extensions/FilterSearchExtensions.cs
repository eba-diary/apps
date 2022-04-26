using Newtonsoft.Json;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public static class FilterSearchExtensions
    {
        public static SavedSearchDto ToDto(this SavedSearch entity)
        {
            return new SavedSearchDto()
            {
                SearchType = entity.SearchType,
                SearchName = entity.SearchName,
                SearchText = entity.SearchText,
                AssociateId = entity.AssociateId,
                FilterCategories = !string.IsNullOrWhiteSpace(entity.FilterCategoriesJson) ? 
                                   JsonConvert.DeserializeObject<List<FilterCategoryDto>>(entity.FilterCategoriesJson) : 
                                   new List<FilterCategoryDto>()
            };
        }

        public static SavedSearch ToEntity(this SavedSearchDto dto)
        {
            return new SavedSearch()
            {
                SearchType = dto.SearchType,
                SearchName = dto.SearchName,
                SearchText = dto.SearchText,
                AssociateId = dto.AssociateId,
                FilterCategoriesJson = dto.FilterCategories != null ? JsonConvert.SerializeObject(dto.FilterCategories) : null
            };
        }
    }
}
