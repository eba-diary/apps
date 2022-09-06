using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class TileSearchDto<T> : FilterSearchDto
    {
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public bool OrderByDescending { get; set; }
        public Func<T, object> OrderByField { get; set; }
        public List<T> SearchableTiles { get; set; }
        public bool UpdateFilters { get; set; }
    }
}
