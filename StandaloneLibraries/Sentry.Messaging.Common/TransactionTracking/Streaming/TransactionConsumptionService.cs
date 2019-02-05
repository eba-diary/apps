using Sentry.Common.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.Messaging.Common
{
    public class TransactionConsumptionService : BaseConsumptionService<MessageTransaction>
    {
        #region Declarations
        private readonly IList<IMessageHandler<MessageTransaction>> _handlers;
        #endregion

        #region BaseConsumptionService Overrides
        protected override void _consumer_MessageReady(object sender, MessageTransaction msg)
        {
            if (msg != null)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                foreach (IMessageHandler<MessageTransaction> handler in _handlers)
                {
                    try
                    {
                        Stopwatch sw2 = new Stopwatch();
                        sw2.Start();
                        handler.Handle(msg);
                        sw2.Stop();
                        Logger.Debug(handler.ToString() + " completed in " + sw2.ElapsedMilliseconds + "ms.");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(handler.ToString() + ": Forwarding of Message Transaction failed. MessageId: " + msg.MessageId, ex);
                    }
                }

                sw.Stop();
                Logger.Debug(msg.MessageId + " handled in " + sw.ElapsedMilliseconds + "ms. ");
            }
        }

        protected override void InitHandler()
        {
            foreach (IMessageHandler<MessageTransaction> handler in _handlers)
            {
                handler.Init();
            }
        }

        protected override void CloseHandler()
        {
            foreach (IMessageHandler<MessageTransaction> handler in _handlers)
            {
                if (!handler.HandleComplete())
                {
                    Logger.Info(handler.ToString() + ": Waiting for handling to complete...");
                }

                while (!handler.HandleComplete())
                {
                    //sleep while any async publishing gets finished up
                    System.Threading.Thread.Sleep(100);
                }

                Logger.Info(handler.ToString() + ": Handling completed.");
            }
        }
        #endregion

        #region Constructors
        public TransactionConsumptionService(IMessageConsumer<MessageTransaction> consumer, 
                                             IList<IMessageHandler<MessageTransaction>> handlers, 
                                             ConsumptionConfig config) : base(consumer, config)
        {
            _handlers = handlers;
        }
        #endregion
    }
}
