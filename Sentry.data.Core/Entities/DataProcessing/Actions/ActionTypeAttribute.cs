using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.DataProcessing.Actions
{
    public class ActionTypeAttribute : Attribute
    {
        public ActionTypeAttribute(DataActionType actionType)
        {
            ActionType = actionType;
        }

        public DataActionType ActionType { get; set; }
    }
}
