using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IAssistanceService
    {
        Task<AddAssistanceResultDto> AddAssistanceAsync(AddAssistanceDto addAssistanceDto);
    }
}
