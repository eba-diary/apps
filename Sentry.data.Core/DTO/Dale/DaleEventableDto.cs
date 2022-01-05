using System.Collections.Generic;

namespace Sentry.data.Core
{
    public abstract class DaleEventableDto
    {
        public DaleEventDto DaleEvent { get; set; }
        public abstract void SetResult(IList<DataInventory> searchResults);
    }
}
