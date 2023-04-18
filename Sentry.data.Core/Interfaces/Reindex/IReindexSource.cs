using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IReindexSource<T>
    {
        bool TryGetNextDocuments(out List<T> documents);
    }
}
