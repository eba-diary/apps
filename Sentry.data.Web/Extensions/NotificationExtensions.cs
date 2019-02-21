using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Sentry.data.Core.GlobalEnums;

namespace Sentry.data.Web
{
    public static class NotificationExtensions
    {


        public static NotificationModel ToWeb(this Core.NotificationModel core)
        {
            return new NotificationModel()
            {
                AllDataAssets = core.AllDataAssets.Select(v => new SelectListItem { Text = v.DisplayName, Value = (v.Id).ToString() }).ToList(),
                AllSeverities = default(NotificationSeverity).ToEnumSelectList(),
                CreateUser = core.CreateUser,
                ExpirationTime = core.ExpirationTime,
                IsActive = core.IsActive,
                Message = core.Message,
                MessageSeverity = core.MessageSeverity,
                MessageSeverityDescription = core.MessageSeverityDescription,
                NotificationId = core.NotificationId,
                StartTime = core.StartTime,
                DataAssetId = core.DataAssetId,
                DataAssetName = core.DataAssetName,
                CanEdit = core.CanEdit
            };
        }

        public static List<NotificationModel> ToWeb(this List<Core.NotificationModel> cores)
        {
            List<NotificationModel> models = new List<NotificationModel>();
            cores.ForEach(x => models.Add(x.ToWeb()));
            return models;
        }


        public static Core.NotificationModel ToCore(this NotificationModel model)
        {
            return new Core.NotificationModel()
            {
                CreateUser = model.CreateUser,
                ExpirationTime = model.ExpirationTime,
                Message = model.Message,
                MessageSeverity = model.MessageSeverity,
                NotificationId = model.NotificationId,
                StartTime = model.StartTime,
                DataAssetId = model.DataAssetId
            };
        }

    }
}