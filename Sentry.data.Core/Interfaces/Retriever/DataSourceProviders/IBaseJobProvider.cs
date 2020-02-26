using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IBaseJobProvider
    {
        void Execute(RetrieverJob job);
        void Execute(RetrieverJob job, string filePath);
        void ConfigureProvider(RetrieverJob job);
    }
}
