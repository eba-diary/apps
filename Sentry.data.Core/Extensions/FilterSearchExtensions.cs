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
                SearchName = entity.SearchName,
                SearchText = entity.SearchText,
                AssociateId = entity.AssociateId,
                FilterCategories = JsonConvert.DeserializeObject<List<FilterCategoryDto>>(entity.FilterCategoriesJson)
            };
        }

        public static SavedSearch ToEntity(this SavedSearchDto dto)
        {
            return new SavedSearch()
            {
                SearchName = dto.SearchName,
                SearchText = dto.SearchText,
                AssociateId = dto.AssociateId,
                FilterCategoriesJson = JsonConvert.SerializeObject(dto.FilterCategories)
            };
        }
    }
}
