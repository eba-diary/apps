using System.Collections.Generic;

namespace Sentry.data.Web.API
{
    public class IndexGlobalDatasetsRequestModel : IRequestModel
    {
        public bool IndexAll { get; set; }
        public List<int> GlobalDatasetIds { get; set; }
    }
}