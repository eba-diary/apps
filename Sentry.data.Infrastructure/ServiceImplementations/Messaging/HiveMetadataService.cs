using Sentry.Messaging.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure
{
    public class HiveMetadataService : IMessageHandler<string>
    {

        #region Constructor
        public HiveMetadataService()      
        {

        }

        public void Handle(string msg)
        {
            throw new NotImplementedException();
        }

        public async Task HandleAsync(string msg)
        {
            using (var Container = Bootstrapper.Container.GetNestedContainer())
            {
                IMessageHandler<string> handler = Container.GetInstance<HiveMetadataHandler>();
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
