using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class IndexGlobalDatasetsDto
    {
        public bool IndexAll { get; set; }
        public List<int> GlobalDatasetIds { get; set; }
    }
}
