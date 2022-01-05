using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public class DaleContainSensitiveResultDto : DaleEventableDto
    {
        public bool DoesContainSensitiveResults { get; set; }

        public override void SetResult(IList<DataInventory> searchResults)
        {
            DoesContainSensitiveResults = searchResults?.Any() == true;
        }
    }
}
