using Sentry.data.Core.Exceptions;

namespace Sentry.data.Core
{
    public interface IHiveOdbcProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="schemaDto"></param>
        /// <param name="rows"></param>
        /// <exception cref="HiveTableViewNotFoundException">Thrown when table or view not found</exception>
        /// <exception cref="HiveQueryException">Thrown when odbc driver throws an error</exception>
        /// <returns></returns>
        System.Data.DataTable GetTopNRows(FileSchemaDto schemaDto, int rows);
    }
}
