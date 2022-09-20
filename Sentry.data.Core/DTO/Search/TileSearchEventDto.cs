namespace Sentry.data.Core
{
    public class TileSearchEventDto : FilterSearchDto
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int SortBy { get; set; }
        public int Layout { get; set; }
        public int TotalResults { get; set; }
    }
}
