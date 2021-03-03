using Sentry.data.Core.Entities.S3;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IDataFlowProvider
    {
        void ExecuteDependencies(string bucket, string key, S3ObjectEvent s3Event);
        Task ExecuteDependenciesAsync(string bucket, string key, S3ObjectEvent s3Event);
        Task ExecuteStepAsync(DataFlowStepEvent stepEvent);
    }
}
