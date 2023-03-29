using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface ISchemaFlowService
    {
        Task<SchemaResultDto> AddSchemaAsync(SchemaFlowDto dto);
        Task<SchemaResultDto> UpdateSchemaAsync(SchemaFlowDto dto);
    }
}