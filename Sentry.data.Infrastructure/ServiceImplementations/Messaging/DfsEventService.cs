using Sentry.data.Core;
using Sentry.Messaging.Common;

namespace Sentry.data.Infrastructure
{
    public class DfsEventService : IMessageHandler<string>
    {
        #region Constructor
        public DfsEventService()
        {

        }

        public void Handle(string msg)
        {
            using (var Container = Bootstrapper.Container.GetNestedContainer())
            {
                IMessageHandler<string> handler = Container.GetInstance<DfsEventHandler>();
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
        #endregion
    }
}
