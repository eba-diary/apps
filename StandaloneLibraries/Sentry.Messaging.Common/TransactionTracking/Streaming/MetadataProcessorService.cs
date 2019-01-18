using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.Common.Logging;

namespace Sentry.Messaging.Common
{
    public class MetadataProcessorService : BaseConsumptionService<HiveMetadataEvent>
    {
        #region Declarations
        private readonly IMessageHandler<HiveMetadataEvent> _handlers;
        #endregion


        protected override void CloseHandler()
        {
            throw new NotImplementedException();
        }

        protected override void InitHandler()
        {
            throw new NotImplementedException();
        }

        protected override void _consumer_MessageReady(object sender, HiveMetadataEvent msg)
        {
            switch (msg.EventType)
            {
                case "HIVE-TABLE-CREATE":
                    HiveMetadataEvent evt = (HiveMetadataEvent)msg;
                    Logger.Debug(evt.ToString());
                    break;
                default:
                    break;
            }
        }

        public MetadataProcessorService(IMessageConsumer<HiveMetadataEvent> consumer,
                                             IMessageHandler<HiveMetadataEvent> handler,
                                             ConsumptionConfig config) : base(consumer, config)
        {
            _handlers = handler;
        }
    }
}
