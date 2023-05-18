using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class SearchSchemaFieldsDto : BaseSearchDto
    {
        public List<int> DatasetIds { get; set; }
    }
}
