using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IReindexService
    {
        Task ReindexAsync();
    }
}
