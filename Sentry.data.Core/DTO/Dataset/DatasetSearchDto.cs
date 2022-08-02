using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class DatasetSearchDto : FilterSearchDto
    {
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public Func<DatasetTileDto, object> OrderByField { get; set; }
        public bool OrderByDescending { get; set; }
        public List<DatasetTileDto> SearchableTiles { get; set; }
    }
}
