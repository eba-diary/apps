using Sentry.Common;
using System;
using System.Collections.Generic;

namespace Sentry.Messaging.Common
{
    public class RawMessage : ITrackableMessage, IPublishable
    {
        #region Constructors
        //a lot of this needs to be provided from the instantiator, because otherwise we need to do too much digging
        //the things generating these messages should have more than enough context to build this data out
        //we might have to get a little crazy with enterprise events, but hopefully the others aren't too bad
        public RawMessage(string payload,
                            string deliverySource,
                            string messageArea,
                            string messageSystemSource,
                            string detailMessageType)
        {
            this.Payload = payload;
            this.DeliverySource = deliverySource;
            this.MessageArea = messageArea;
            this.DetailedMessageType = detailMessageType;
            this.MessageSystemSource = messageSystemSource;
        }
        #endregion

        #region Properties
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Payload { get; set; }
        public DateTime PublishDate { get; set; } = SystemClock.Now();
        public string DeliverySource { get; set; }
        public string MessageArea { get; set; }
        public string MessageSystemSource { get; set; }
        public string DetailedMessageType { get; set; }
        public string TrackKey
        {
            get
            {
                if (this.Payload == null) return "";

                //TODO: Come up with a better track key
                return this.Payload.Length > 350 ? this.Payload.Substring(0, 350) : this.Payload;
            }
        }

        public bool IsXML
        {
            get
            {
                return (this.Payload != null && this.Payload.StartsWith("<"));
            }
        }

        public bool IsJSON
        {
            get
            {
                return (this.Payload != null && (this.Payload.StartsWith("{") || this.Payload.StartsWith("[")));
            }
        }
        #endregion

        #region "ITrackableMessage Implementation"
        IDictionary<string, string> ITrackableMessage.GetIdentifyingAttributes()
        {
            return new Dictionary<string, string>() { { "PayloadPreview", this.TrackKey } };
        }

        string ITrackableMessage.GetMessageArea()
        {
            return this.MessageArea;
        }

        DateTime ITrackableMessage.GetMessageDate()
        {
            return SystemClock.Now();
        }

        string ITrackableMessage.GetMessageDeliverySource()
        {
            return this.DeliverySource;
        }

        string ITrackableMessage.GetMessageDetailedType()
        {
            return this.DeliverySource + "." + this.MessageSystemSource + "." + this.DetailedMessageType;
        }

        string ITrackableMessage.GetMessageId()
        {
            return this.Id.ToString();
        }

        IEnumerable<string> ITrackableMessage.GetMessageStepIds()
        {
            return new List<string> { this.Id.ToString() };
        }

        string ITrackableMessage.GetMessageSystemSource()
        {
            return this.MessageSystemSource;
        }

        string ITrackableMessage.GetMessageType()
        {
            return "Raw_Message";
        }

        string ITrackableMessage.GetOriginalMessageId()
        {
            return this.Id.ToString();
        }

        string ITrackableMessage.GetSerializedMessage()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
        #endregion
    }
}
