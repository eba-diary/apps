using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class DataInventoryAssetCategoriesDto : DataInventoryEventableDto
    {
        public List<DataInventoryCategoryDto> DataInventoryCategories { get; set; }
    }
}
