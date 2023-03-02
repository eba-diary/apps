using Sentry.data.Core.Exceptions;
using Sentry.data.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IConfigService : IEntityService
    {
        List<string> Validate(FileSchemaDto dto);
        List<string> Validate(DataSourceDto dto);
        List<string> Validate(DatasetFileConfigDto dto);
        bool UpdateandSaveOAuthToken(HTTPSSource source, string newToken, DateTime tokenExpTime);
        bool CreateAndSaveNewDataSource(DataSourceDto dto);
        bool UpdateAndSaveDataSource(DataSourceDto dto);
        /// <summary>
        /// Creates a new DatasetFileConfig entity object and adds it to domain context.
        /// <para>Domain context changes are not saved.</para>
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        int Create(DatasetFileConfigDto dto);
        bool CreateAndSaveDatasetFileConfig(DatasetFileConfigDto dto);
        void UpdateDatasetFileConfig(DatasetFileConfigDto dto);
        bool UpdateAndSaveDatasetFileConfig(DatasetFileConfigDto dto);
        DataSourceDto GetDataSourceDto(int Id);
        UserSecurity GetUserSecurityForDataSource(int id);
        Task<AccessRequest> GetDataSourceAccessRequest(int dataSourceId);

        Task<string> RequestAccessToDataSource(AccessRequest request);


        /// <summary>
        /// Return DastasetFileConfig Dto object
        /// </summary>
        /// <param name="configId"></param>
        /// <returns></returns>
        /// <exception cref="SchemaUnauthorizedAccessException">Thrown when user does not have access to schema</exception>
        DatasetFileConfigDto GetDatasetFileConfigDto(int configId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datasetId"></param>
        /// <returns></returns>
        /// <exception cref="DatasetUnauthorizedAccessException">Thrown when user does not have access to dataset</exception>
        /// <exception cref="DatasetNotFoundException">Thrown when user does not have access to dataset</exception>
        List<DatasetFileConfigDto> GetDatasetFileConfigDtoByDataset(int datasetId);
        UserSecurity GetUserSecurityForConfig(int id);
        /// <summary>
        /// Generates necessary trigger to regenerate consumption layer components.
        /// </summary>
        /// <param name="datasetId">Non-Zero value required</param>
        /// <param name="schemaId"></param>
        /// <exception cref="SchemaUnauthorizedAccessException">When DatasetFileConfig is already marked for deletion</exception>
        /// <returns></returns>
        bool SyncConsumptionLayer(int datasetId, int schemaId);

        /// <summary>
        /// Returns Schema data flow for given schema
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        Tuple<DataFlowDetailDto, List<RetrieverJob>> GetDataFlowForSchema(DatasetFileConfig config);

        /// <summary>
        /// Returns the Schema data flow for given schema
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        Tuple<List<RetrieverJob>, List<DataFlowStepDto>> GetDataFlowDropLocationJobs(DatasetFileConfig config);

        /// <summary>
        /// Returns a list of Producer data flows for given schema
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        List<Tuple<DataFlowDetailDto, List<RetrieverJob>>> GetExternalDataFlowsBySchema(DatasetFileConfig config);
    }
}