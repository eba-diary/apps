using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IConfigService
    {
        SchemaDTO GetSchemaDTO(int id);
        SchemaDetailDTO GetSchemaDetailDTO(int id);
        IList<ColumnDTO> GetColumnDTO(int id);
        void UpdateFields(int configId, int schemaId, List<SchemaRow> schemaRows);
        List<string> Validate(DataElementDto dto);
        List<string> Validate(DataSourceDto dto);
        List<string> Validate(DatasetFileConfigDto dto);
        bool UpdateandSaveOAuthToken(HTTPSSource source, string newToken, DateTime tokenExpTime);
        bool CreateAndSaveNewDataSource(DataSourceDto dto);
        bool UpdateAndSaveDataSource(DataSourceDto dto);
        bool CreateAndSaveDatasetFileConfig(DatasetFileConfigDto dto);
        bool UpdateAndSaveDatasetFileConfig(DatasetFileConfigDto dto);
        DataSourceDto GetDataSourceDto(int Id);
        UserSecurity GetUserSecurityForDataSource(int id);
        AccessRequest GetDataSourceAccessRequest(int dataSourceId);
        string RequestAccessToDataSource(AccessRequest request);
        DatasetFileConfigDto GetDatasetFileConfigDto(int configId);
        bool Delete(int id);
        UserSecurity GetUserSecurityForConfig(int id);
    }
}