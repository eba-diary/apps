namespace Sentry.data.Core
{
    public interface ISnowProvider
    {
        System.Data.DataTable GetTopNRows(string db, string schema, string table, int rows);
        bool CheckIfExists(string db, string schema, string table);

    }
}
