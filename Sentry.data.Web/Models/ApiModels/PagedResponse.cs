using Sentry.data.Core.Helpers.Paginate;
using System.Collections.Generic;

namespace Sentry.data.Web.Models.ApiModels
{
    public class PagedResponse<T>
    {
        public PagedResponse(PagedList<T> items)
        {
            Records = new List<T>();
            Records.AddRange(items);
            Metadata = new PagedResponseMetadata<T>(items);
        }

        public List<T> Records { get; internal set; }
        public PagedResponseMetadata<T> Metadata { get; internal set; }
    }
}