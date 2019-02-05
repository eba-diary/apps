using System;
using System.Collections.Generic;

namespace Sentry.Messaging.Common
{
    public interface IQueueStatisticManager
    {
        bool IsUp(string queueName, string consumerName, string producerName, int millisecondWindow);
        IEnumerable<QueueConsumerStatistic> GetConsumerStatisticsBy(DateTime start, DateTime end, string queueName, string consumerName);
        IEnumerable<QueueProducerStatistic> GetProducerStatisticsBy(DateTime start, DateTime end, string queueName, string producerName);
        void Remove(DateTime asOf);
    }
}
