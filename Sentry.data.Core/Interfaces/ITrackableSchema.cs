using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface ITrackableSchema
    {
        bool IsSchemaTracked { get; set; }
        List<FileSchema> SchemaRevisions { get; set; }
    }
}
