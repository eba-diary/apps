using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Interfaces.DataProcessing
{
    public interface IDataStep
    {
        string Name { get; set; }
        string TargetStoragePrefix { get; set; }
        string TargetStorageBucket { get; set; }
    }
}
