using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IConfigService
    {
        SchemaDTO GetSchemaDTO(int id);
        IList<ColumnDTO> GetColumnDTO(int id);
        void UpdateFields(int configId, int schemaId, List<SchemaRow> schemaRows);
    }
}