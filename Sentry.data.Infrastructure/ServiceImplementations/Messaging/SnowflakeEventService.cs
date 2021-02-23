using Sentry.Messaging.Common;
using System;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class SnowflakeEventService : IMessageHandler<string>
    {
        #region Constructor
        public SnowflakeEventService()
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
                IMessageHandler<string> handler = Container.GetInstance<SnowflakeEventHandler>();
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
