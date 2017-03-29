using NHibernate;

namespace Sentry.data.Infrastructure
{
    /// <summary>
    /// This class is here just for purposes of deleting all data from the demo database.
    /// Your production application should NEVER do something like this!!!
    /// </summary>
    /// <remarks></remarks>
    public class DemoDataService
    {
        public static void DeleteAllDemoData(ISession session)
        {

            ISQLQuery query = session.CreateSQLQuery(
                "delete from CategorizedAsset " +
                "delete from Category " +
                "delete from Asset " +
                "delete from [User]");

            query.ExecuteUpdate();

        }
    }
}
