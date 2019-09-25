using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface ISchemaService
    {
        SchemaRevisionDto GetSchemaRevisionDto(int id);
        List<SchemaRevisionDto> GetSchemaRevisionDtoBySchema(int id);
        List<BaseFieldDto> GetBaseFieldDtoBySchemaRevision(int revisionId);
    }
}
