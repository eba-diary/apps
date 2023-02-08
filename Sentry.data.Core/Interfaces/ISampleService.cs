using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface ISampleService
    {
        Task<SampleDto> GetSample(int id);
        Task<SampleDto> AddSample(SampleDto dto);
        Task<SampleDto> UpdateSample(SampleDto dto);
    }
}
