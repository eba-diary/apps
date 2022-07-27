using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class DatasetSearchResultDto
    {
        public int TotalResults { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public List<DatasetTileDto> Tiles { get; set; } = new List<DatasetTileDto>();
        public List<FilterCategoryDto> FilterCategories { get; set; } = new List<FilterCategoryDto>();
    }
}
