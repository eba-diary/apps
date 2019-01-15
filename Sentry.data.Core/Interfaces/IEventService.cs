using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IEventService
    {
        Task CreateEventAsync(Event e);
    }
}