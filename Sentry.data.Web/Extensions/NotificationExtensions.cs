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
            List<SelectListItem> AreaList = new List<SelectListItem>();
            foreach (var item in core.AllDataAssets)
            {
                AreaList.Add(new SelectListItem { Text = item.DisplayName, Value = Core.GlobalConstants.Notifications.DATAASSET_TYPE + "_" + item.Id });
            }
            foreach (var item in core.AllBusinessAreas)
            {
                AreaList.Add(new SelectListItem { Text = item.Name, Value = Core.GlobalConstants.Notifications.BUSINESSAREA_TYPE + "_" + item.Id });
            }

            return new NotificationModel()
            {
                AllDataAssets = AreaList,
                AllSeverities = default(NotificationSeverity).ToEnumSelectList(),
                CreateUser = core.CreateUser,
                ExpirationTime = core.ExpirationTime,
                IsActive = core.IsActive,
                Message = core.Message,
                MessageSeverity = core.MessageSeverity,
                MessageSeverityDescription = core.MessageSeverityDescription,
                NotificationId = core.NotificationId,
                StartTime = core.StartTime,
                ObjectId = core.ObjectId.ToString(),
                ObjectName = core.ObjectName,
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
            int Id = int.Parse(model.ObjectId.Substring(model.ObjectId.IndexOf('_') + 1));
            string objectType = model.ObjectId.Substring(0, model.ObjectId.IndexOf('_'));

            return new Core.NotificationModel()
            {
                CreateUser = model.CreateUser,
                ExpirationTime = model.ExpirationTime,
                Message = model.Message,
                MessageSeverity = model.MessageSeverity,
                NotificationId = model.NotificationId,
                StartTime = model.StartTime,
                ObjectId = Id,
                NotificationType = objectType
            };
        }

        public static SystemNotificationModel ToWeb(this List<Core.NotificationModel> cores)
        {
            SystemNotificationModel model = new SystemNotificationModel
            {
                CriticalNotifications = new List<SystemNotificationItemModel>(),
                StandardNotifications = new List<SystemNotificationItemModel>()
            };

            foreach (var item in cores.Where(w => w.MessageSeverity == NotificationSeverity.Danger)
            {
                model.CriticalNotifications.Add(new SystemNotificationItemModel()
                {
                    Title = "Title",
                    NotificationDate = item.StartTime.ToString(),
                    Message = item.Message
                });
            };

            model.CriticalNotifications.Add(new SystemNotificationItemModel
            {
                Title = "Driver Assignment Data Issue!",
                Message = "Driver Assignments are incorrect in SERA PL and ODS for December only. Measures impacted are inforce counts and loss information. This will impact reports like the insured profile and customer personas. Total inforce counts and loss information are correct, but how the mesasure is associated to the driver is incorrect. This doesn&rsquo;t impact measures for exposures and premium. Defects have been opened to address and additional communication will be sent when the fix has been implemented.",
                NotificationDate = DateTime.Today.ToShortDateString()
            });
        }

    }
}