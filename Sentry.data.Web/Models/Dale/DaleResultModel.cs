using Sentry.data.Core;
using System.Collections.Generic;


namespace Sentry.data.Web
{
    public class DaleResultModel
    {
        public List<DataInventorySearchResultRowModel> DaleResults { get; set; }
        public DataInventoryEventDto DaleEvent { get; set; }

    }
}
