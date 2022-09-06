﻿namespace Sentry.data.Core
{
    public interface ISnowProvider
    {
        System.Data.DataTable GetTopNRows(string db, string schema, string table, int rows);
        bool CheckIfExists(string db, string schema, string table);
        System.Data.DataTable GetExceptRows(string sourceDb, string targetDb, string schema, string table, string queryParameter, AuditSearchType auditSearchType);

        System.Data.DataTable GetCompareRows(string sourceDb, string targetDb, string schema, string table, string queryParameter, AuditSearchType auditSearchType);
    }
}
