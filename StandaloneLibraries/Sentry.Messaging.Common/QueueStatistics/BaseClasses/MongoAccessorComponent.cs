using MongoDB.Driver;

namespace Sentry.Messaging.Common
{
    public class MongoAccessorComponent
    {
        #region "declarations"
        private readonly string _mongoConnectionString;
        private readonly string _databaseName;

        private IMongoDatabase _database;
        #endregion

        #region "properties"
        public IMongoDatabase Database
        {
            get
            {
                if (_database == null)
                {
                    IMongoClient client = new MongoClient(_mongoConnectionString);
                    _database = client.GetDatabase(_databaseName);
                }

                return _database;
            }
        }
        #endregion

        #region "constructors"
        public MongoAccessorComponent(string mongoConnectionString,
                                      string databaseName)
        {
            _mongoConnectionString = mongoConnectionString;
            _databaseName = databaseName;
        }
        #endregion


    }
}
