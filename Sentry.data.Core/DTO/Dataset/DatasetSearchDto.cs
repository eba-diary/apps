using System;

namespace Sentry.data.Core
{
    public class DatasetSearchDto : FilterSearchDto
    {
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public Func<DatasetTileDto, object> OrderByField { get; set; }
        public bool OrderByDescending { get; set; }
    }
}
