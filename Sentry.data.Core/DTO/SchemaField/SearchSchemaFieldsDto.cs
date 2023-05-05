using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class SearchSchemaFieldsDto
    {
        public string SearchText { get; set; }
        public List<int> DatasetIds { get; set; }
    }
}
