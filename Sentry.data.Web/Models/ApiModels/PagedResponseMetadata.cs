using Sentry.data.Core.Helpers.Paginate;
using System.Text.Json.Serialization;

namespace Sentry.data.Web.Models.ApiModels
{
    public class PagedResponseMetadata<T>
    {
        public PagedResponseMetadata(PagedList<T> items)
        {
            CurrentPage = items.CurrentPage;
            TotalPages = items.TotalPages;
            PageSize = items.PageSize;
            TotalCount = items.TotalCount;
            HasPrevious = items.HasPrevious;
            HasNext = items.HasNext;
        }
        [JsonPropertyName("current_page")]
        public int CurrentPage { get; set; }
        [JsonPropertyName("total_pages")]
        public int TotalPages { get; set; }
        [JsonPropertyName("page_size")]
        public int PageSize { get; set; }
        [JsonPropertyName("total_count")]
        public int TotalCount { get; set; }
        [JsonPropertyName("has_previous")]
        public bool HasPrevious { get; set; }
        [JsonPropertyName("has_next")]
        public bool HasNext { get; set; }
    }
}