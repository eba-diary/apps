using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IGlobalDatasetAdminService
    {
        Task<IndexGlobalDatasetsResultDto> IndexGlobalDatasetsAsync(IndexGlobalDatasetsDto indexGlobalDatasetsDto);
    }
}
