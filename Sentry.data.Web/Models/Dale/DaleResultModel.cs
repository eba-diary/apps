using Sentry.data.Core;
using System.Collections.Generic;


namespace Sentry.data.Web
{
    public class DaleResultModel
    {
        public List<DaleResultRowModel> DaleResults { get; set; }
        public DaleEventDto DaleEvent { get; set; }

    }
}
