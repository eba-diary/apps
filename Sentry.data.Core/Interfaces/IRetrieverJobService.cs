using Hangfire;

namespace Sentry.data.Core
{
    public interface IRetrieverJobService
    {
        void RunRetrieverJob(int JobId, IJobCancellationToken token, string filePath = null);
    }
}
