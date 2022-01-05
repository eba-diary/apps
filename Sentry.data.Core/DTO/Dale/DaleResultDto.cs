using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public class DaleResultDto : DaleEventableDto
    {
        public List<DaleResultRowDto> DaleResults { get; set; }

        public override void SetResult(IList<DataInventory> searchResults)
        {
            DaleResults = searchResults.Select(x => x.ToDto()).ToList();
        }
    }
}
