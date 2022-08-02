using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class DatasetSearchModel : FilterSearchModel
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int SortBy { get; set; }
        public int Layout { get; set; }
        public List<TileModel> SearchableTiles { get; set; }
    }
}