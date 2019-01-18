namespace Sentry.Messaging.Common
{
    public class QueueProducerStatistic : QueueStatistic
    {
        public string ProducerName { get; set; }        
        public string Server { get; set; }
        public string Port { get; set; }
        public string InternalId { get; set; }
        public QueueLatency RoundTripTime { get; protected set; } = new QueueLatency();
        public QueueLatency InternalPublishTime { get; protected set; } = new QueueLatency();
    }

    public class QueueLatency
    {
        public int MinimumTime { get; set; }
        public int MaximumTime { get; set; }
        public int AverageTime { get; set; }
        public int SumTime { get; set; }
        public int SamplePoints { get; set; }
    }
}
