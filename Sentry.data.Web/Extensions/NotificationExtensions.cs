using System;
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
                ObjectId = core.ObjectId,
                ObjectName = core.ObjectName,
                CanEdit = core.CanEdit,
                Title = core.Title,
                ObjectType = core.NotificationType
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
                ObjectId = model.ObjectId.Split('_')[1],
                NotificationType = model.ObjectId.Split('_')[0],
                Title = model.Title
            };
        }

        //public static SystemNotificationModel ToWeb(this List<Core.NotificationModel> cores)
        //{
        //    SystemNotificationModel model = new SystemNotificationModel
        //    {
        //        CriticalNotifications = new List<SystemNotificationItemModel>(),
        //        StandardNotifications = new List<SystemNotificationItemModel>()
        //    };

        //    foreach (var item in cores.Where(w => w.MessageSeverity == NotificationSeverity.Danger))
        //    {
        //        model.CriticalNotifications.Add(new SystemNotificationItemModel()
        //        {
        //            Title = "Title",
        //            NotificationDate = item.StartTime.ToString(),
        //            Message = item.Message
        //        });
        //    };

        //    model.CriticalNotifications.Add(new SystemNotificationItemModel
        //    {
        //        Title = "Driver Assignment Data Issue!",
        //        Message = "Driver Assignments are incorrect in SERA PL and ODS for December only. Measures impacted are inforce counts and loss information. This will impact reports like the insured profile and customer personas. Total inforce counts and loss information are correct, but how the mesasure is associated to the driver is incorrect. This doesn&rsquo;t impact measures for exposures and premium. Defects have been opened to address and additional communication will be sent when the fix has been implemented.",
        //        NotificationDate = DateTime.Today.ToShortDateString()
        //    });
        //}

        public static SystemNotificationModel ToModel(this List<Core.NotificationModel> models)
        {
            SystemNotificationModel model = new SystemNotificationModel();

            foreach (var notification in models.Where(w => w.IsActive && w.MessageSeverity == NotificationSeverity.Danger))
            {
                model.CriticalNotifications.Add(notification.ToModel());
            }

            foreach (var notificaiton in models.Where(w => w.IsActive && w.MessageSeverity != NotificationSeverity.Danger))
            {
                model.StandardNotifications.Add(notificaiton.ToModel());
            }

            return model;
        }

        public static SystemNotificationItemModel ToModel(this Core.NotificationModel notification)
        {
            return new SystemNotificationItemModel()
            {
                Title = (String.IsNullOrWhiteSpace(notification.Title)) ? "Alert" : notification.Title,
                Message = notification.Message,
                NotificationDate = notification.StartTime.ToShortDateString()
            };
        }

        //private SystemNotificationModel BuildMockNotifications()
        //{
        //    List<NotificationModel>

        //    SystemNotificationModel model = new SystemNotificationModel
        //    {
        //        CriticalNotifications = new List<SystemNotificationItemModel>(),
        //        StandardNotifications = new List<SystemNotificationItemModel>()
        //    };

        //    model.CriticalNotifications.Add(new SystemNotificationItemModel
        //    {
        //        Title = "Driver Assignment Data Issue!",
        //        Message = "Driver Assignments are incorrect in SERA PL and ODS for December only. Measures impacted are inforce counts and loss information. This will impact reports like the insured profile and customer personas. Total inforce counts and loss information are correct, but how the mesasure is associated to the driver is incorrect. This doesn&rsquo;t impact measures for exposures and premium. Defects have been opened to address and additional communication will be sent when the fix has been implemented.",
        //        NotificationDate = DateTime.Today.ToShortDateString()
        //    });

        //    model.CriticalNotifications.Add(new SystemNotificationItemModel
        //    {
        //        Title = "A Second Critical Alert!",
        //        Message = "This is a second critical alert to demo how this would look and act in DSC.",
        //        NotificationDate = DateTime.Today.AddDays(-1).ToShortDateString()
        //    });

        //    model.StandardNotifications.Add(new SystemNotificationItemModel
        //    {
        //        Title = "Example of a Standard Notification",
        //        Message = "This is just a standard notification. It will live underneath the critical notifications and in a carousel.",
        //        NotificationDate = DateTime.Today.AddHours(-1).ToShortDateString()
        //    });

        //    model.StandardNotifications.Add(new SystemNotificationItemModel
        //    {
        //        Title = "A Second Standard Notification",
        //        Message = "This is just another standard notification. It will live underneath the critical notifications and in a carousel.",
        //        NotificationDate = DateTime.Today.AddDays(-2).ToShortDateString()
        //    });

        //    return model;
        //}

    }
}