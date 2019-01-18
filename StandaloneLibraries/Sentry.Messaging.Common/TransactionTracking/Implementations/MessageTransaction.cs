using Sentry.AsyncCommandProcessor;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Sentry.Messaging.Common
{
    public class MessageTransaction : MessageTransactionLite, ICommand, IPublishable
    {
        #region Declarations      
        private IList<string> _messageStepIds = new List<string>();
        private IList<KeyValuePair<string, string>> _identifyingAttributes = new List<KeyValuePair<string, string>>();
        #endregion

        #region Properties
        public string Action { get;  set; }
        public string Application { get;  set; }
        public string MessageId { get;  set; }
        public string MessageType { get;  set; }
        public string MessageArea { get;  set; }
        public string MessageSystemSource { get;  set; }
        public string MessageDeliverySource { get;  set; }
        public string SerializedMessage { get;  set; }
        public string StatusCode { get;  set; }
        public string Detail { get;  set; }
        public string Environment { get;  set; }

        public IEnumerable<string> MessageStepIds
        {
            get
            {
                return _messageStepIds.AsEnumerable();
            }
            protected set
            {
                _messageStepIds = value.ToList();
            }
        }

        public IEnumerable<KeyValuePair<string, string>> IdentifyingAttributes
        {
            get
            {
                return _identifyingAttributes.AsEnumerable();
            }
            protected set
            {
                _identifyingAttributes = value.ToList();
            }
        }

        #endregion

        #region Methods
        public string Decompress()
        {
            byte[] gzBuffer = Convert.FromBase64String(this.SerializedMessage);
            using (MemoryStream ms = new MemoryStream())
            {
                int msgLength = BitConverter.ToInt32(gzBuffer, 0);
                ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

                byte[] buffer = new byte[msgLength];

                ms.Position = 0;
                using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    zip.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }

        protected string Compress(string input)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(input);
            MemoryStream ms = new MemoryStream();
            using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true))
            {
                zip.Write(buffer, 0, buffer.Length);
            }

            ms.Position = 0;

            byte[] compressed = new byte[ms.Length];
            ms.Read(compressed, 0, compressed.Length);

            byte[] gzBuffer = new byte[compressed.Length + 4];
            System.Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
            System.Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);

            return Convert.ToBase64String(gzBuffer);
        }

        protected bool DetermineToSerialize()
        {
            switch (MessageTransactionTrackingAccess.SerializationOption)
            {
                case SerializeMessageOptions.SerializeEndActions:
                    if (Action == MessageActionCodes.MessageActionBegin) return false;
                    else return true;
                case SerializeMessageOptions.SerializeOriginalMessageOnly:
                    if (OriginalMessageId == MessageId && Action == MessageActionCodes.MessageActionBegin) return true;
                    else return false;
                case SerializeMessageOptions.AlwaysSerialize:
                default:
                    return true;
            }
        }
        #endregion

        #region Constructors
        public MessageTransaction(string action, ITrackableMessage msg) : base()
        {
            this.Action = action;
            this.MessageId = msg.GetMessageId();
            this.OriginalMessageId = msg.GetOriginalMessageId();
            this.MessageType = msg.GetMessageType();
            this.MessageArea = msg.GetMessageArea();
            this.MessageSystemSource = msg.GetMessageSystemSource();
            this.MessageDeliverySource = msg.GetMessageDeliverySource();
            this.MessageDetailedType = msg.GetMessageDetailedType();
            this.MessageDate = msg.GetMessageDate();
            this.SerializedMessage = DetermineToSerialize() ? this.Compress(msg.GetSerializedMessage()) : "";
            this.Environment = MessageTransactionTrackingAccess.EnvironmentProvider.Invoke();
            this.Application = MessageTransactionTrackingAccess.MessagingApplication;

            this.MessageStepIds = msg.GetMessageStepIds();
            this.IdentifyingAttributes = msg.GetIdentifyingAttributes();
        }

        public MessageTransaction(string action, ITrackableMessage msg, string detail) : this(action, msg)
        {
            this.Detail = detail;
        }

        public MessageTransaction(string action, ITrackableMessage msg, string code, string detail) : this(action, msg, detail)
        {
            this.StatusCode = code;
        }

        public MessageTransaction() { }
        #endregion
    }
}
