using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class TileSearchResultDto<T>
    {
        public int TotalResults { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public List<T> Tiles { get; set; } = new List<T>();
        public List<FilterCategoryDto> FilterCategories { get; set; } = new List<FilterCategoryDto>();
    }
}
