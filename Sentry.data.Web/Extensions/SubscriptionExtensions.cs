using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public static class SubscriptionExtensions
    {
        public static Core.SubscriptionDto ToDto(this SubscriptionModel model)
        {
            if (model == null) 
            { 
                return new Core.SubscriptionDto(); 
            }

            Core.SubscriptionDto dto = new Core.SubscriptionDto();
            dto.group = model.group;
            dto.datasetID = model.datasetID;
            dto.businessAreaID = model.businessAreaID;
            dto.SentryOwnerName = model.SentryOwnerName;
            dto.CurrentSubscriptions = model.CurrentSubscriptions;
            dto.CurrentSubscriptionsBusinessArea = model.CurrentSubscriptionsBusinessArea;
                        
            return dto;
        }
    }
}