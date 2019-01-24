using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IEventService
    {
        Task CreateViewSchemaEditSuccessEvent(int configId, string userId, string reason);
    }
}