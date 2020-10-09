using Sentry.data.Core.Exceptions;
using System.Data.Odbc;

namespace Sentry.data.Core
{
    public interface IHiveOdbcProvider
    {
        bool CheckTableExists(OdbcConnection conn, string table);

        bool CheckViewExists(OdbcConnection conn, string view);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schemaDto"></param>
        /// <param name="rows"></param>
        /// <exception cref="HiveTableViewNotFoundException">Thrown when table or view not found</exception>
        /// <exception cref="HiveQueryException">Thrown when odbc driver throws an error</exception>
        /// <returns></returns>
        System.Data.DataTable GetTopNRows(OdbcConnection conn, string table, int rows);

        OdbcConnection GetConnection(string database);
    }
}
