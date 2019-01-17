using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IEventService
    {
        Task CreateViewedSuccessEvent(int configId, int datasetId, string userId, string reason);
    }
}