using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IHszFileWatchService
    {
        void OnStart(RetrieverJob job, CancellationToken token);
    }
}
