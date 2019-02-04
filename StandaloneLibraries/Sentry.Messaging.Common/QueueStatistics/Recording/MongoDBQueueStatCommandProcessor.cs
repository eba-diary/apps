using Sentry.AsyncCommandProcessor;

namespace Sentry.Messaging.Common
{
    public class MongoDbQueueStatCommandProcessor : AsyncCommandProcessor.AsyncCommandProcessor
    {
        #region "declarations"
        private readonly string _destinationCollection;
        private readonly MongoAccessorComponent _dataAccess;
        #endregion
        
        #region "AsyncCommandProcessor Implementation"
        protected override void ProcessCommand(ICommand cmd)
        {
            //expects a queue stat command
            if(cmd != null && cmd is QueueStatistic)
            {
                QueueStatistic stat = (QueueStatistic)cmd;

                _dataAccess.Database.GetCollection<QueueStatistic>(_destinationCollection).InsertOne(stat);
            }            
        }
        #endregion

        #region "constructors"
        public MongoDbQueueStatCommandProcessor(int pollWait,
                                                string mongoConnectionString,
                                                string databaseName,
                                                string destinationCollection) : base(pollWait)
        {
            _dataAccess = new MongoAccessorComponent(mongoConnectionString, databaseName);
            _destinationCollection = destinationCollection;
        }
        #endregion
    }
}
