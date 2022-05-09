using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class DataInventorySearchResultDto : DataInventoryEventableDto
    {
        public long SearchTotal { get; set; }
        public List<DataInventorySearchResultRowDto> DataInventoryResults { get; set; } = new List<DataInventorySearchResultRowDto>();
    }
}
