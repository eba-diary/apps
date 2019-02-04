using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Sentry.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.Messaging.Common
{
    public class MongoDbQueueStatisticManager : IQueueStatisticManager
    {
        #region "declarations"
        private readonly string _destinationCollection;
        private readonly MongoAccessorComponent _dataAccess;
        #endregion

        #region "IQueueStatisticManager Implementation"
        IEnumerable<QueueConsumerStatistic> IQueueStatisticManager.GetConsumerStatisticsBy(DateTime start, DateTime end, string queueName, string consumerName)
        {
            //some quick validation
            if(start == DateTime.MinValue ||
               end == DateTime.MinValue ||
               string.IsNullOrEmpty(queueName) ||
               string.IsNullOrEmpty(consumerName))
            {
                throw new InvalidOperationException("Invalid Request for statistics.  Start date, end date, queue name, and consumer name must be provided!");
            }

            //UTC Adjust
            start = start.AddHours(5);
            end = end.AddHours(5);

            return _dataAccess.Database.GetCollection<QueueConsumerStatistic>(_destinationCollection).AsQueryable().Where((x) => x.RecordDate <= end && x.RecordDate >= start && x.QueueName  == queueName && x.ConsumerName == consumerName).ToList();
        }

        void IQueueStatisticManager.Remove(DateTime asOf)
        {
            BsonDocument delete = new BsonDocument();
            delete.Add("RecordDate", new BsonDocument("$lt", asOf));

            _dataAccess.Database.GetCollection<QueueStatistic>(_destinationCollection).DeleteMany(delete);
        }

        bool IQueueStatisticManager.IsUp(string queueName, string consumerName, string producerName, int millisecondWindow)
        {
            DateTime dte = SystemClock.Now();

            //UTC Adjust
            dte = dte.AddHours(5);
            dte = dte.AddMilliseconds(millisecondWindow * -1);

            if(!string.IsNullOrEmpty(consumerName))
            {
                return _dataAccess.Database.GetCollection<QueueConsumerStatistic>(_destinationCollection).AsQueryable().Where((x) => x.RecordDate >= dte && x.QueueName == queueName && x.ConsumerName == consumerName).Any();
            }
            else
            {
                return _dataAccess.Database.GetCollection<QueueProducerStatistic>(_destinationCollection).AsQueryable().Where((x) => x.RecordDate >= dte && x.QueueName == queueName && x.ProducerName == producerName).Any();
            }
            
        }

        IEnumerable<QueueProducerStatistic> IQueueStatisticManager.GetProducerStatisticsBy(DateTime start, DateTime end, string queueName, string producerName)
        {
            //some quick validation
            if (start == DateTime.MinValue ||
               end == DateTime.MinValue ||
               string.IsNullOrEmpty(queueName) ||
               string.IsNullOrEmpty(producerName))
            {
                throw new InvalidOperationException("Invalid Request for statistics.  Start date, end date, and producer name must be provided!");
            }

            //UTC Adjust
            start = start.AddHours(5);
            end = end.AddHours(5);

            return _dataAccess.Database.GetCollection<QueueProducerStatistic>(_destinationCollection).AsQueryable().Where((x) => x.RecordDate <= end && x.RecordDate >= start && x.QueueName == queueName && x.ProducerName == producerName).ToList();

        }
        #endregion

        #region "constructors"
        public MongoDbQueueStatisticManager(string mongoConnectionString,
                                            string databaseName,
                                            string destinationCollection)
        {
            _dataAccess = new MongoAccessorComponent(mongoConnectionString, databaseName);
            _destinationCollection = destinationCollection;
        }
        #endregion
    }
}
