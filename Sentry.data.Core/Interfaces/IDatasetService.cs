using Sentry.Core;
using Sentry.data.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IDatasetService : IEntityService
    {
        Task<ValidationException> Validate(DatasetDto dto);
        int CreateAndSaveNewDataset(DatasetDto dto);
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

        /// <summary>
        /// Retrieve all the permissions granted to the dataset with the given <paramref name="datasetId"/>.
        /// </summary>
        DatasetPermissionsDto GetDatasetPermissions(int datasetId);
        string SetDatasetFavorite(int datasetId, string associateId);
        SecurityTicket GetLatestInheritanceTicket(int datasetId);
        List<string> GetDatasetNamesForAsset(string asset);
    }
}
