using Newtonsoft.Json.Linq;

namespace Sentry.data.Core
{
    public interface IGoogleBigQueryService
    {
        void UpdateSchemaFields(int schemaId, JArray bigQueryFields);
    }
}
