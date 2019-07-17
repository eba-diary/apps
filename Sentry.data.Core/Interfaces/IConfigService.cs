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
        bool UpdateandSaveOAuthToken(HTTPSSource source, string newToken, DateTime tokenExpTime);
        bool CreateAndSaveNewDataSource(DataSourceDto dto);
        bool UpdateAndSaveDataSource(DataSourceDto dto);
        DataSourceDto GetDataSourceDto(int Id);
    }
}