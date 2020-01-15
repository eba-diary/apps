using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public static class SubscriptionExtensions
    {
        public static Core.SubscriptionModelDTO ToDto(this SubscriptionModel model)
        {
            if (model == null) 
            { 
                return new Core.SubscriptionModelDTO(); 
            }

            Core.SubscriptionModelDTO dto = new Core.SubscriptionModelDTO();
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