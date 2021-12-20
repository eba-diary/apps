using Sentry.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IDatasetService
    {
        Task<ValidationException> Validate(DatasetDto dto);
        int CreateAndSaveNewDataset(DatasetDto dto);
        bool Delete(int datasetId, bool logicalDelete = true);
        DatasetDto GetDatasetDto(int id);
        List<DatasetDto> GetAllDatasetDto();
        DatasetDetailDto GetDatesetDetailDto(int id);
        IDictionary<int, string> GetDatasetList();
        void UpdateAndSaveDataset(DatasetDto dto);
        UserSecurity GetUserSecurityForDataset(int datasetId);
        UserSecurity GetUserSecurityForConfig(int configId);
        Task<AccessRequest> GetAccessRequestAsync(int datasetId);
        string RequestAccessToDataset(AccessRequest request);
        List<Dataset> GetDatasetsForQueryTool();
        List<Dataset> GetDatasetMarkedDeleted();
        List<DatasetSummaryMetadataDTO> GetDatasetSummaryMetadataDTO();
    }
}
