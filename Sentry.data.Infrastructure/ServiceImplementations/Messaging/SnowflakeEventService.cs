using Sentry.Messaging.Common;

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
            using (var Container = Bootstrapper.Container.GetNestedContainer())
            {
                IMessageHandler<string> handler = Container.GetInstance<SnowflakeEventHandler>();
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
