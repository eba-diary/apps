using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public static class NotificationExtensions
    {


        public static NotificationModel ToWeb(this Core.NotificationModel core)
        {
            return new NotificationModel()
            {
                AllDataAssets = core.AllDataAssets.Select(v => new SelectListItem { Text = v.DisplayName, Value = (v.Id).ToString() }).ToList(),
                AllSeverities = core.AllSeverities.Select(v => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList(),
                CreateUser = core.CreateUser,
                ExpirationTime = core.ExpirationTime,
                IsActive = core.IsActive,
                Message = core.Message,
                MessageSeverity = core.MessageSeverity,
                MessageSeverityTag = core.MessageSeverityTag,
                NotificationId = core.NotificationId,
                StartTime = core.StartTime
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
                MessageSeverityTag = model.MessageSeverityTag,
                NotificationId = model.NotificationId,
                StartTime = model.StartTime
            };
        }

    }
}