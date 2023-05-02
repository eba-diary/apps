using Sentry.Core;
using Sentry.data.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IDatasetService : IEntityService
    {
        Task<ValidationException> ValidateAsync(DatasetSchemaDto dto);
        Task<DatasetResultDto> AddDatasetAsync(DatasetDto datasetDto);
        int Create(DatasetDto dto);
        /// <summary>
        /// Performs all necessary external dependency creation statements.
        /// </summary>
        /// <remarks>To be executed after creation of dataset</remarks>
        /// <param name="datasetId"></param>
        void CreateExternalDependencies(int datasetId);
        int CreateAndSaveNewDataset(DatasetSchemaDto dto);
        DatasetDto GetDatasetDto(int id);

        /// <summary>
        /// Returns combination of dataset and schema metadata as a dto object.
        /// For datasetdto only metdata, use <see cref="GetDatasetDto(int)"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        DatasetSchemaDto GetDatasetSchemaDto(int id);
        List<DatasetSchemaDto> GetAllDatasetDto();
        List<DatasetSchemaDto> GetAllActiveDatasetDto();
        DatasetDetailDto GetDatasetDetailDto(int id);
        IDictionary<int, string> GetDatasetList();
        Task<DatasetResultDto> UpdateDatasetAsync(DatasetDto dto);
        void UpdateAndSaveDataset(DatasetSchemaDto dto);
        UserSecurity GetUserSecurityForDataset(int datasetId);
        UserSecurity GetUserSecurityForConfig(int configId);
        Task<AccessRequest> GetAccessRequestAsync(int datasetId);
        Task<string> RequestAccessToDataset(AccessRequest request);
        List<Dataset> GetDatasetsForQueryTool();
        List<Dataset> GetDatasetMarkedDeleted();
        List<DatasetSummaryMetadataDTO> GetDatasetSummaryMetadataDTO();

        /// <summary>
        /// Retrieve all the permissions granted to the dataset with the given <paramref name="datasetId"/>.
        /// </summary>
        DatasetPermissionsDto GetDatasetPermissions(int datasetId);
        string SetDatasetFavorite(int datasetId, string associateId, bool removeForAllEnvironments);
        SecurityTicket GetLatestInheritanceTicket(int datasetId);
        List<string> GetDatasetNamesForAsset(string asset);
        List<string> GetInheritanceEnabledDatasetNamesForAsset(string asset);
        List<Dataset> GetInheritanceEnabledDatasetsForAsset(string asset);

        Task<string> RequestAccessRemoval(AccessRequest request);

        IQueryable<DatasetFile> GetDatasetFileTableQueryable(int configId);

        /// <summary>
        /// Does the dataset exist in the target environment
        /// </summary>
        /// <param name="datasetName"></param>
        /// <param name="saidAssetKey"></param>
        /// <param name="targetNamedEnvironment"></param>
        /// <returns></returns>
        (int targetDatasetId, bool datasetExistsInTarget) DatasetExistsInTargetNamedEnvironment(string datasetName, string saidAssetKey, string targetNamedEnvironment);
    }
}
