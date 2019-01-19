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
            if (!_handlers.HandleComplete())
            {
                Logger.Info(_handlers.ToString() + ": Waiting for handling to complete...");
            }

            while (!_handlers.HandleComplete())
            {
                //sleep while any async publishing gets finished up
                System.Threading.Thread.Sleep(100);
            }

            Logger.Info(_handlers.ToString() + ": Handling completed.");
        }

        protected override void InitHandler()
        {
            _handlers.Init();
        }

        protected override void _consumer_MessageReady(object sender, HiveMetadataEvent msg)
        {
            switch (msg.EventType)
            {
                case "HIVE-TABLE-CREATE":                    
                    Logger.Debug(msg.ToString());
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
