using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IAuditService
    {
        BaseAuditDto GetNonParquetFiles(int datasetId, int schemaId, string queryParameter, AuditSearchType auditSearchType);
        BaseAuditDto GetComparedRowCount(int datasetId, int schemaId, string queryParameter, AuditSearchType auditSearchType);
    }
}
