using Sentry.AsyncCommandProcessor;
using Sentry.Common;
using System;

namespace Sentry.Messaging.Common
{
    public class QueueStatistic: BaseTransaction, ICommand
    {
        #region "properties"
        //thinking ahead - having this parsed for grouping instead of "group by date part"
        public int RecordDateDay { get; protected set; }
        public int RecordDateMonth { get; protected set; }
        public int RecordDateYear { get; protected set; }
        public int RecordDateHour { get; protected set; }
        public int RecordDateMinute { get; protected set; }
        public int RecordDateSecond { get; protected set; }
        public DateTime RecordDate { get; protected set; }

        public string QueueName { get; set; }        
        public string QueueType { get; set; }
        public string Environment { get; set; }
        #endregion

        #region "constructors"
        protected QueueStatistic()
        {
            this.RecordDate = SystemClock.Now();

            this.RecordDateDay = this.RecordDate.Day;
            this.RecordDateYear = this.RecordDate.Year;
            this.RecordDateMonth = this.RecordDate.Month;
            this.RecordDateHour = this.RecordDate.Hour;
            this.RecordDateMinute = this.RecordDate.Minute;
            this.RecordDateSecond = this.RecordDate.Second;

            this.Id = Guid.NewGuid();
        }
        #endregion
    }
}
