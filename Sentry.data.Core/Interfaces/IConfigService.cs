using System;
using System.Collections.Generic;
using Sentry.data.Core.Exceptions;

namespace Sentry.data.Core
{
    public interface IConfigService
    {
        SchemaApiDTO GetSchemaApiDTO(int id);
        SchemaDetaiApilDTO GetSchemaDetailDTO(int id);
        IList<ColumnDTO> GetColumnDTO(int id);
        void UpdateFields(int configId, int schemaId, List<SchemaRow> schemaRows, string jsonSchemaObject = null);
        List<string> Validate(FileSchemaDto dto);
        List<string> Validate(DataSourceDto dto);
        List<string> Validate(DatasetFileConfigDto dto);
        bool UpdateandSaveOAuthToken(HTTPSSource source, string newToken, DateTime tokenExpTime);
        bool CreateAndSaveNewDataSource(DataSourceDto dto);
        bool UpdateAndSaveDataSource(DataSourceDto dto);
        bool CreateAndSaveDatasetFileConfig(DatasetFileConfigDto dto);
        bool UpdateAndSaveDatasetFileConfig(DatasetFileConfigDto dto);
        List<DatasetFileConfig> GetSchemaMarkedDeleted();
        DataSourceDto GetDataSourceDto(int Id);
        UserSecurity GetUserSecurityForDataSource(int id);
        AccessRequest GetDataSourceAccessRequest(int dataSourceId);
        string RequestAccessToDataSource(AccessRequest request);
        DatasetFileConfigDto GetDatasetFileConfigDto(int configId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datasetId"></param>
        /// <returns></returns>
        /// <exception cref="DatasetUnauthorizedAccessException">Thrown when user does not have access to dataset</exception>
        /// <exception cref="DatasetNotFoundException">Thrown when user does not have access to dataset</exception>
        List<DatasetFileConfigDto> GetDatasetFileConfigDtoByDataset(int datasetId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">DatasetFileConfig identifier</param>
        /// <param name="logicalDelete">Perform soft or hard delete</param>
        /// <param name="parentDriven">Is this driven by parent object</param>
        /// <returns></returns>
        /// <exception cref="DatasetFileConfigDeletedException">When DatasetFileConfig is already marked for deletion</exception>
        bool Delete(int id, bool logicalDelete = true, bool parentDriven = false);
        UserSecurity GetUserSecurityForConfig(int id);
        /// <summary>
        /// Generates necessary trigger to regenerate consumption layer components.
        /// </summary>
        /// <param name="datasetId">Non-Zero value required</param>
        /// <param name="schemaId"></param>
        /// <returns></returns>
        bool SyncConsumptionLayer(int datasetId, int schemaId);
    }
}