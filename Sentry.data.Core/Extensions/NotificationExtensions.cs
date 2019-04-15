using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public static class NotificationExtensions
    {

        public static NotificationModel ToModel(this Notification core)
        {
            NotificationModel model = new NotificationModel()
            {
                CreateUser = core.CreateUser,
                ExpirationTime = core.ExpirationTime,
                Message = core.Message,
                MessageSeverity = core.MessageSeverity,
                MessageSeverityDescription = core.MessageSeverity.ToString(),
                NotificationId = core.NotificationId,
                StartTime = core.StartTime,
                IsActive = core.StartTime <= DateTime.Now && core.ExpirationTime > DateTime.Now,
                NotificationType = core.NotificationType,
                ObjectId = core.ParentObject.ToString(),
                Title = core.Title                
            };

            //switch (core.NotificationType)
            //{
            //    case GlobalConstants.Notifications.BUSINESSAREA_TYPE:
            //        model.objectId = GlobalConstants.Notifications.BUSINESSAREA_TYPE + "_" + core.NotificationId.ToString();
            //        break;
            //    case GlobalConstants.Notifications.DATAASSET_TYPE:
            //        model.ObjectId = GlobalConstants.Notifications.DATAASSET_TYPE + "_" + core.NotificationId.ToString();
            //        break;
            //    default:
            //        break;
            //}

            return model;
        }

        public static List<NotificationModel> ToModels(this List<Notification> cores, IDatasetContext domainContext, ISecurityService securityService, UserService userService)
        {

            IApplicationUser user = userService.GetCurrentUser();
            List<NotificationModel> models = new List<NotificationModel>();

            foreach (var notification in cores)
            {
                NotificationModel model = notification.ToModel();

                switch (model.NotificationType)
                {
                    case GlobalConstants.Notifications.DATAASSET_TYPE:
                        DataAsset da = domainContext.GetById<DataAsset>(notification.ParentObject);
                        model.ObjectName = da.DisplayName;
                        UserSecurity us = securityService.GetUserSecurity(da, user);
                        model.CanEdit = us.CanModifyNotifications;
                        break;
                    case GlobalConstants.Notifications.BUSINESSAREA_TYPE:
                        BusinessArea ba = domainContext.GetById<BusinessArea>(notification.ParentObject);
                        model.ObjectName = ba.Name;
                        //UserSecurity us = _securityService.GetUserSecurity(notification.ParentObject, user);
                        //model.CanEdit = us.CanModifyNotifications;
                        model.CanEdit = true;
                        break;
                    default:
                        break;
                }
                models.Add(model);
            }
            return models;
        }

        //public static List<NotificationModel> ToModels(this IQueryable<Notification> cores)
        //{
        //    return cores.ToList().ToModels();
        //}


        public static Notification ToCore(this NotificationModel model)
        {
            return new Notification()
            {
                CreateUser = model.CreateUser,
                ExpirationTime = model.ExpirationTime,
                Message = model.Message,
                MessageSeverity = model.MessageSeverity,
                NotificationId = model.NotificationId,
                StartTime = model.StartTime,
                NotificationType = model.NotificationType,
                ParentObject = int.Parse(model.ObjectId),
                Title = model.Title
            };
        }
    }
}
