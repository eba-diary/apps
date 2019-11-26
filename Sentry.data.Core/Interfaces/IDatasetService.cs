using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IDatasetService
    {
        List<string> Validate(DatasetDto dto);
        int CreateAndSaveNewDataset(DatasetDto dto);
        bool Delete(int datasetId, bool logicalDelete = true);
        DatasetDto GetDatasetDto(int id);
        List<DatasetDto> GetAllDatasetDto();
        DatasetDetailDto GetDatesetDetailDto(int id);
        void UpdateAndSaveDataset(DatasetDto dto);
        UserSecurity GetUserSecurityForDataset(int datasetId);
        UserSecurity GetUserSecurityForConfig(int configId);
        AccessRequest GetAccessRequest(int datasetId);
        string RequestAccessToDataset(AccessRequest request);
        List<Dataset> GetDatasetsForQueryTool();
        List<Dataset> GetDatasetMarkedDeleted();
    }
}
