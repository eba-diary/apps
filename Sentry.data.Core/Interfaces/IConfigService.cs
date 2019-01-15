using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IConfigService
    {
        void UpdateFields(int configId, int schemaId, List<SchemaRow> schemaRows);
    }
}