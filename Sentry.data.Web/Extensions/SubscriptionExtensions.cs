using System.Collections.Generic;

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
            dto.CurrentSubscriptionsBusinessArea = model.SubscriptionsBusinessAreas;
            
            return dto;
        }

        public static List<Core.BusinessAreaSubscription> ToDto(this List<Web.BusinessAreaSubscriptionModel> modelList)
        {
            if (modelList == null)
            {
                return new List<Core.BusinessAreaSubscription>();
            }

            List<Core.BusinessAreaSubscription> dtos = new List<Core.BusinessAreaSubscription>();
            modelList.ForEach(x => dtos.Add(x.ToDto()));
            return dtos;
        }

        public static Core.BusinessAreaSubscription ToDto(this Web.BusinessAreaSubscriptionModel model)
        {
            if (model == null)
            {
                return new Core.BusinessAreaSubscription();
            }

            Core.BusinessAreaSubscription dto = new Core.BusinessAreaSubscription(model.BusinessAreaType, model.EventType, model.Interval, model.SentryOwnerName);
            dto.ID = model.ID;
            dto.ChildrenSelections = model.ChildrenSelections;
            dto.Children = model.SubscriptionsBusinessAreaModels.ToDto();
            return dto;
        }


        public static List<BusinessAreaSubscriptionModel> ToWeb(this List<Core.BusinessAreaSubscription> coreList)
        {
            if(coreList == null)
            {

                return new List<BusinessAreaSubscriptionModel>(); 
            }
            
            List<BusinessAreaSubscriptionModel> models = new List<BusinessAreaSubscriptionModel>();
            coreList.ForEach(x => models.Add(x.ToWeb()));
            return models;
        }


        public static BusinessAreaSubscriptionModel ToWeb(this Core.BusinessAreaSubscription core)
        {
            if(core == null)
            {
                return new BusinessAreaSubscriptionModel();
            }

            BusinessAreaSubscriptionModel model = new BusinessAreaSubscriptionModel(core.BusinessAreaType, core.EventType, core.Interval, core.SentryOwnerName);
            model.ID = core.ID;
            model.ChildrenSelections = core.ChildrenSelections;
            model.Children = core.Children.ToWeb();
            return model;
        }
    }
}