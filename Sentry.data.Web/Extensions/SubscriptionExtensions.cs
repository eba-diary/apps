using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public static class SubscriptionExtensions
    {
        public static Core.SubscriptionModelDto ToDto(this SubscriptionModel model)
        {
            if (model == null) 
            { 
                return new Core.SubscriptionModelDto(); 
            }

            Core.SubscriptionModelDto dto = new Core.SubscriptionModelDto();
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