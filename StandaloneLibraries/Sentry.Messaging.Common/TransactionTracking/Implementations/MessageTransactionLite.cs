using Sentry.DataRetentionPolicy;
using System;

namespace Sentry.Messaging.Common
{
    public class MessageTransactionLite : BaseTransaction, ISummarizable
    {
        public string OriginalMessageId { get;  set; }
        public string MessageDetailedType { get;  set; }
        public virtual DateTime MessageDate { get;  set; }
        public bool IsSummarized { get; set; }

        string ISummarizable.DataId { get { return Id.ToString(); } set { Id = Guid.Parse(value); } }
        DateTime ISummarizable.DataDate { get { return MessageDate; } set { MessageDate = value; } }
        string ISummarizable.JoiningId { get { return OriginalMessageId; } set { OriginalMessageId = value; } }
        string ISummarizable.SplitFieldValue { get { return MessageDetailedType; } set { MessageDetailedType = value; } }
        bool ISummarizable.IsSummarized { get { return IsSummarized; } set { IsSummarized = value; } }

        public MessageTransactionLite(Guid id, DateTime messageDate, string originalMessageId, string messageDetailedType, bool isSummarized)
        {
            this.Id = id;
            this.MessageDate = messageDate;
            this.OriginalMessageId = originalMessageId;
            this.MessageDetailedType = messageDetailedType;
            this.IsSummarized = isSummarized;
        }
        public MessageTransactionLite()
        {
            this.Id = Guid.NewGuid();
        }

    }
}
