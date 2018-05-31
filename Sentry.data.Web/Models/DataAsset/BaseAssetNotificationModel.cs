using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Sentry.Associates;
using Sentry.data.Infrastructure;

namespace Sentry.data.Web
{
    public class BaseAssetNotificationModel
    {
        public BaseAssetNotificationModel()
        {
        }

        public BaseAssetNotificationModel(AssetNotifications an, IAssociateInfoProvider associateInfoService)
        {
            this.MessageSeveritiy = an.MessageSeverity;
            //this.DataAssetId = an.DataAssetId;
            this.ParentDataAssetName = an.ParentDataAsset.DisplayName;
            this.ExpirationTime = an.ExpirationTime;
            this.StartTime = an.StartTime;
            this.CreateUser = an.CreateUser;
            this.Message = an.Message;
            this.NotificationId = an.NotificationId;
            this.DisplayMessage = an.DisplayMessage;
            this.MessageSeverityTag = an.MessageSeverityTag;
            this.DisplayCreateUser = associateInfoService.GetAssociateInfo(an.CreateUser);
        }
        
        public int MessageSeveritiy { get; set; }
        //public int DataAssetId { get; set; }
        public string ParentDataAssetName { get; set; }

        [DisplayName("Expiration Time")]
        //[DataType(System.ComponentModel.DataAnnotations.DataType.DateTime)]
        public DateTime ExpirationTime { get; set; }

        [DisplayName("Start Time")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.DateTime)]
        public DateTime StartTime { get; set; }
        public string CreateUser { get; set; }
        [DisplayName("Creator")]
        public Associate DisplayCreateUser { get; set; }
        public string Message { get; set; }
        public int NotificationId { get; set; }
        public string DisplayMessage { get; set; }
        public string MessageSeverityTag { get; set; }
        public string EditHref
        {
            get
            {
                string href = null;
                href = $"<a href=\"/DataAsset/EditAssetNotification?notificationId={NotificationId}\">Edit</a>";
                return href;
            }
        }
        public Boolean IsActive
        {
            get
            {
                if(StartTime < DateTime.Now && ExpirationTime > DateTime.Now)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}