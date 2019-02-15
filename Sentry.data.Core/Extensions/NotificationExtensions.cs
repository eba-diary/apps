using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public static class NotificationExtensions
    {

        public static NotificationModel ToModel(this AssetNotifications core)
        {
            return new NotificationModel()
            {
                CreateUser = core.CreateUser,
                ExpirationTime = core.ExpirationTime,
                Message = core.Message,
                MessageSeverity = core.MessageSeverity,
                MessageSeverityDescription = core.MessageSeverity.ToString(),
                NotificationId = core.NotificationId,
                StartTime = core.StartTime,
                DataAssetId = core.ParentDataAsset.Id,
                DataAssetName = core.ParentDataAsset.Name,
                IsActive = core.StartTime <= DateTime.Now && core.ExpirationTime > DateTime.Now
            };
        }

        public static List<NotificationModel> ToModels(this List<AssetNotifications> cores)
        {
            List<NotificationModel> models = new List<NotificationModel>();
            cores.ForEach(x => models.Add(x.ToModel()));
            return models;
        }

        public static List<NotificationModel> ToModels(this IQueryable<AssetNotifications> cores)
        {
            return cores.ToList().ToModels();
        }


        public static AssetNotifications ToCore(this NotificationModel model)
        {
            return new AssetNotifications()
            {
                CreateUser = model.CreateUser,
                ExpirationTime = model.ExpirationTime,
                Message = model.Message,
                MessageSeverity = model.MessageSeverity,
                NotificationId = model.NotificationId,
                StartTime = model.StartTime
            };
        }

    }
}
