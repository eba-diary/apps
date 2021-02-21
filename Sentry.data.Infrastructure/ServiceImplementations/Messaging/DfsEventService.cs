using Sentry.data.Core;
using Sentry.Messaging.Common;
using Sentry.Common.Logging;

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
                IDataFeatures _featureFlags = Container.GetInstance<IDataFeatures>();
                if (!_featureFlags.CLA2671_DFSEVENTEventHandler.GetValue())
                {
                    IMessageHandler<string> handler = Container.GetInstance<DfsEventHandler>();
                    handler.Handle(msg);
                }
                else
                {
                    Logger.Info($"DfsEventService skipping event - CLA2671_DFSEVENTEventHandler:{_featureFlags.CLA2671_DFSEVENTEventHandler.GetValue().ToString()}");
                }
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
