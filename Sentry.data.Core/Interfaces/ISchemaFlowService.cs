using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface ISchemaFlowService
    {
        Task<SchemaResultDto> AddSchemaAsync(AddSchemaDto dto);
        Task<SchemaResultDto> UpdateSchemaAsync(UpdateSchemaDto dto);
    }
}