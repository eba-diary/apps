namespace Sentry.data.Core
{
    public interface ISnowProvider
    {
        //System.Data.DataTable GetTopNRows(OdbcConnection conn, string table, int rows);
        void GetTopNRows();
        //System.Security.SecureString GetSecureString(string str);

    }
}
