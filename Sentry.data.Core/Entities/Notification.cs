using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using System;

namespace Sentry.data.Core
{    
    public class Notification : IValidatable
    {
        public virtual NotificationSeverity MessageSeverity { get; set; }
        public virtual DateTime ExpirationTime { get; set; }
        public virtual DateTime StartTime { get; set; }
        public virtual string CreateUser { get; set; }
        public virtual string Message { get; set; }
        public virtual int NotificationId { get; set; }
        public virtual int ParentObject { get; set; }
        public virtual string NotificationType { get; set; }
        public virtual string Title { get; set; }
        
        public virtual string DisplayMessage
        {
            get
            {
                switch (MessageSeverity)
                {
                    case NotificationSeverity.Critical:
                        return $"<strong class=\"alertHeading\">Alert!</strong> {Message}";
                    case NotificationSeverity.Warning:
                        return $"<strong class=\"alertHeading\">Warning!</strong> {Message}";
                    case NotificationSeverity.Info:
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
            if (!Enum.IsDefined(typeof(NotificationSeverity), MessageSeverity))
            {
                vr.Add(ValidationErrors.invalidSeverity, "The severity is not valid");
            }
            if (ParentObject == null)
            {
                vr.Add(ValidationErrors.parentAssetIsNull, "The Asset is required");
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
            public const string invalidSeverity = "invalidSeverity";
            public const string parentAssetIsNull = "parentAssetIsNull";
        }
    }
}
