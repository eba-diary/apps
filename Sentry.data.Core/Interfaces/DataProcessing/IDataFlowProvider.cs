using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core.Entities.S3;

namespace Sentry.data.Core
{
    public interface IDataFlowProvider
    {
        Task ExecuteDependenciesAsync(string bucket, string key, S3ObjectEvent s3Event);
        Task ExecuteDependenciesAsync(S3ObjectEvent s3e);
        Task ExecuteStep(DataFlowStepEvent stepEvent);
    }
}
