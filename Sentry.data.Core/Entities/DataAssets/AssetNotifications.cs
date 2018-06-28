using Sentry.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{    
    public class AssetNotifications : IValidatable
    {
        public virtual int MessageSeverity { get; set; }
        public virtual DateTime ExpirationTime { get; set; }
        public virtual DateTime StartTime { get; set; }
        public virtual string CreateUser { get; set; }
        public virtual string Message { get; set; }
        public virtual int NotificationId { get; set; }
        public virtual DataAsset ParentDataAsset { get; set; }

        public virtual string DisplayMessage
        {
            get
            {
                switch (MessageSeverityTag.ToLower())
                {
                    case "danger":
                        return $"<strong class=\"alertHeading\">Alert!</strong> {Message}";
                    case "warning":
                        return $"<strong class=\"alertHeading\">Warning!</strong> {Message}";
                    case "info":
                        return $"<strong class=\"alertHeading\">Info: </strong> {Message}";
                    default :
                        return Message;
                }
            }
        }
        public virtual string MessageSeverityTag
        {
            get
            {
                 return Enum.GetName(typeof(NotificationSeverity), this.MessageSeverity);
         
            }
        }
        public virtual ValidationResults ValidateForSave()
        {
            ValidationResults vr = new ValidationResults();
            if (string.IsNullOrWhiteSpace(CreateUser))
            {
                vr.Add(ValidationErrors.emptyCreateUser, "The Create User is required");
            }
            if (string.IsNullOrWhiteSpace(Message))
            {
                vr.Add(ValidationErrors.messageIsBlank, "The message is required");
            }
            if (ExpirationTime < StartTime)
            {
                vr.Add(ValidationErrors.expireDateBeforeStartDate, "The Expiration Date must be after the Start Date");
            }

            return vr;
        }

        public virtual ValidationResults ValidateForDelete()
        {
            return new ValidationResults();
        }

        public class ValidationErrors
        {
            public const string messageIsBlank = "messageIsBlank";
            public const string emptyCreateUser = "emptyCreateUser";
            public const string expireDateBeforeStartDate = "expireDateBeforeStartDate";
        }
    }
}
