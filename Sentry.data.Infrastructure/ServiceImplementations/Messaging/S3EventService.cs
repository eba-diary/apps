using Sentry.data.Core;
using Sentry.Messaging.Common;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class S3EventService : IMessageHandler<string>
    {
        #region Constructor
        public S3EventService()
        {

        }

        public void Handle(string msg)
        {
            using (var Container = Bootstrapper.Container.GetNestedContainer())
            {
                IMessageHandler<string> handler = Container.GetInstance<S3EventHandler>();
                handler.Handle(msg);
            }
        }

        public async Task HandleAsync(string msg)
        {
            using (var Container = Bootstrapper.Container.GetNestedContainer())
            {
                IMessageHandler<string> handler = Container.GetInstance<S3EventHandler>();
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
        #endregion
    }
}
