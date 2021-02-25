using Sentry.Common.Logging;
using Sentry.Messaging.Common;
using StructureMap;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class MetadataProcessorProvider : BaseConsumptionService<string>
    {
        #region Declarations
        private readonly IList<IMessageHandler<string>> _handlers;
        #endregion


        protected override void CloseHandler()
        {
            foreach (IMessageHandler<string> handler in _handlers)
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

        protected override void InitHandler()
        {
            foreach(IMessageHandler<string> handler in _handlers)
            {
                handler.Init();
            }            
        }

        protected override void _consumer_MessageReady(object sender, string msg)
        {
            List<Task> TaskList = new List<Task>();
            using (IContainer container = Bootstrapper.Container.GetNestedContainer())
            {
                IList<IMessageHandler<string>> handlerList = container.GetInstance<IList<IMessageHandler<string>>>();

                foreach (var handler in handlerList)
                {
                    TaskList.Add(handler.HandleAsync(msg));
                }

                Task.WaitAll(TaskList.ToArray());
            }

            //Parallel.ForEach(_handlers, (h) => h.Handle(msg));
        }

        public MetadataProcessorProvider(IMessageConsumer<string> consumer,
                                             IList<IMessageHandler<string>> handler,
                                             ConsumptionConfig config) : base(consumer, config)
        {
            _handlers = handler;
        }
    }
}
