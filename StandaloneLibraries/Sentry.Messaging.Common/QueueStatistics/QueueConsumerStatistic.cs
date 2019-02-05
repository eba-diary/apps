namespace Sentry.Messaging.Common
{
    public class QueueConsumerStatistic: QueueStatistic
    {
        
        public string QueueSection { get; set; } //kafka partition
        public int TotalMessagesAvailable { get; set; }
        public int MessagesLeftToConsume { get; set; }
        public int TotalMessageByteSize { get; set; }
        public string ConsumerName { get; set; }


        #region "constructors"
        public QueueConsumerStatistic() : base()
        {

        }
        #endregion

    }
}
