using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IAuditService
    {
        BaseAuditDto GetExceptRows(int datasetId, int schemaId);

        BaseAuditDto GetRowCountCompare(int datasetId, int schemaId);
    }
}
