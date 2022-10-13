using Hangfire;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IRetrieverJobService
    {
        void RunRetrieverJob(int JobId, IJobCancellationToken token, string filePath = null);
        Task UpdateJobStatesAsync();
    }
}
