using Sentry.data.Core;
using Sentry.Messaging.Common;

namespace Sentry.data.Infrastructure
{
    public class DataStepProcessorService : IMessageHandler<string>
    {
        public DataStepProcessorService()
        {

        }
        public void Handle(string msg)
        {
            using (var Container = Bootstrapper.Container.GetNestedContainer())
            {
                IMessageHandler<string> handler = Container.GetInstance<DataStepProcessorHandler>();
                handler.Handle(msg);
            }
        }

        public bool HandleComplete()
        {
            return true;
        }

        public void Init()
        {
            //do nothing
        }
    }
}
