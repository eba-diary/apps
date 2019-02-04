using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IDatasetService
    {
        List<string> Validate(DatasetDto dto);
        int CreateAndSaveNewDataset(DatasetDto dto);
        DatasetDto GetDatasetDto(int id);
        DatasetDetailDto GetDatesetDetailDto(int id);

        void UpdateAndSaveDataset(DatasetDto dto);

        UserSecurity GetUserSecurityForDataset(int datasetId);
        UserSecurity GetUserSecurityForConfig(int configId);
        AccessRequest GetAccessRequest(int datasetId);
        string RequestAccessToDataset(AccessRequest request);
    }
}
