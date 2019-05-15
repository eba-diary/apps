using Sentry.Messaging.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class HszDropLocationCreateModel : BaseEventMessage
    {
        public HszDropLocationCreateModel()
        {
            EventType = "HSZ-DROPLOC-CREATE-REQUESTED";
        }

        public string DropLocation { get; set; }

    }
}
