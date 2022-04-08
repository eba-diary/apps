using Sentry.Messaging.Common;
using System;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class SparkConverterEventService : IMessageHandler<string>
    {
        #region Constructor
        public SparkConverterEventService()
        {

        }
        #endregion

        public void Handle(string msg)
        {
            throw new NotImplementedException();
        }

        public async Task HandleAsync(string msg)
        {
            using (var Container = Bootstrapper.Container.GetNestedContainer())
            {
                IMessageHandler<string> handler = Container.GetInstance<SparkConverterEventHandler>();
                await handler.HandleAsync(msg).ConfigureAwait(false);
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
