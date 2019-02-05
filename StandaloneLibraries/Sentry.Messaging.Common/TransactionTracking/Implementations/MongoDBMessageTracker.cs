using Sentry.AsyncCommandProcessor;

namespace Sentry.Messaging.Common
{
    public class MongoDbMessageTracker : IMessageTracker
    {
        #region Declarations       
        private readonly MongoDbMessageTrackingStorage _queue;
        #endregion

        #region Properties
        private MongoDbMessageTrackingStorage Queue
        {
            get
            {
                this.StartIfNecessary();

                return _queue;
            }
        }
        #endregion

        #region Contructors
        public MongoDbMessageTracker(string mongoConnectionString,
                                     string databaseName,
                                     string destinationCollection)
        {
            //int waitTime = (destinationCollection == "WebServiceTransaction" ? 5 : 100);
            _queue = new MongoDbMessageTrackingStorage(5, mongoConnectionString, databaseName, destinationCollection);
        }
        #endregion

        #region IMessageTracker Implementation
        public void TrackMessageProcessingBegin(ITrackableMessage msg)
        {
            this.Queue.QueueCommand(new MessageTransaction(MessageActionCodes.MessageActionBegin, msg));
        }

        public void TrackMessageProcessingFailure(ITrackableMessage msg, string code, string detail)
        {
            this.Queue.QueueCommand(new MessageTransaction(MessageActionCodes.MessageActionFailure, msg, code, detail));
        }

        public void TrackMessageProcessingSkip(ITrackableMessage msg, string detail)
        {
            this.Queue.QueueCommand(new MessageTransaction(MessageActionCodes.MessageActionSkip, msg, detail));
        }

        public void TrackMessageProcessingSuccess(ITrackableMessage msg)
        {
            this.Queue.QueueCommand(new MessageTransaction(MessageActionCodes.MessageActionSuccess, msg));
        }

        public void RunOffPendingTransactions()
        {
            Sentry.Common.Logging.Logger.Info("Running off pending transactions: Begin");
            this.Queue.RunOff();
            Sentry.Common.Logging.Logger.Info("Running off pending transactions: End");
        }
        #endregion

        #region Methods
        protected void StartIfNecessary()
        {
            if (!_queue.IsStarted()) _queue.StartPolling();
        }
        #endregion
    }

    class MongoDbMessageTrackingStorage : AsyncCommandProcessor.AsyncCommandProcessor
    {
        #region Declarations
        private readonly string _destinationCollection;
        private readonly MongoAccessorComponent _dataAccess;
        #endregion

        #region AsyncCommandProcessor Implementation
        protected override void ProcessCommand(ICommand cmd)
        {
            //expects a message tx command
            if (cmd != null && cmd is MessageTransaction)
            {
                MessageTransaction tx = (MessageTransaction)cmd;
                _dataAccess.Database.GetCollection<MessageTransaction>(_destinationCollection).InsertOne(tx);
                //Sentry.Common.Logging.Logger.Debug("MessageId: " + tx.MessageId + " " + tx.Action + " processed.");
            }
        }
        #endregion

        #region Constructors
        public MongoDbMessageTrackingStorage(int pollWait,
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
