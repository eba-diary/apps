using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.Messaging.Common
{
    public abstract class TrackableMessage<T> : ITrackableMessage
    {
        #region Declarations
        protected string _serializedMessage;
        protected string _idString;
        #endregion

        #region Properties
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OriginalMessageId { get; set; }
        public T Message { get; set; }
        public string MessageType { get; set; }
        public string MessageDeliverySource { get; set; }
        public string MessageArea { get; set; }
        public string MessageSystemSource { get; set; }
        public string DetailedMessageType { get; set; }
        public IEnumerable<string> MessageStepIds { get; set; } = new List<string>();
        public IDictionary<string, string> IdentifyingAttributes { get; set; } = new Dictionary<string, string>();
        #endregion

        #region Must Override
        protected abstract void BuildTrackableMessage();
        #endregion

        #region ITrackableMessage Implementation
        IDictionary<string, string> ITrackableMessage.GetIdentifyingAttributes()
        {
            return IdentifyingAttributes;
        }

        string ITrackableMessage.GetMessageArea()
        {
            return MessageArea;
        }

        DateTime ITrackableMessage.GetMessageDate()
        {
            return Sentry.Common.SystemClock.Now();
        }

        string ITrackableMessage.GetMessageDeliverySource()
        {
            return MessageDeliverySource;
        }

        string ITrackableMessage.GetMessageDetailedType()
        {
            return MessageDeliverySource + "." + MessageSystemSource + "." + DetailedMessageType;
        }

        string ITrackableMessage.GetMessageId()
        {
            return MessageTransactionTrackingAccess.MessagingIdPrefix + Id.ToString();
        }

        IEnumerable<string> ITrackableMessage.GetMessageStepIds()
        {
            return MessageStepIds;
        }

        string ITrackableMessage.GetMessageSystemSource()
        {
            return MessageSystemSource;
        }

        string ITrackableMessage.GetMessageType()
        {
            return MessageType;
        }

        string ITrackableMessage.GetOriginalMessageId()
        {
            return OriginalMessageId.ToString();
        }

        string ITrackableMessage.GetSerializedMessage()
        {
            return _serializedMessage;
        }
        #endregion

        #region Constructors
        protected TrackableMessage(T msg)
        {
            Message = msg;
        }
        #endregion
    }
}
