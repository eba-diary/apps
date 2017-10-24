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
        private int _notificationId;
        private string _message;
        private string _createUser;
        private DateTime _startTime;
        private DateTime _expirationTime;
        //private int _dataAssetId;
        private int _messageSeverity;
        private string _messageSeverityTag;
        private DataAsset _parentDataAsset;

        public virtual int MessageSeverity
        {
            get { return _messageSeverity; }
            set { _messageSeverity = value; }
        }
        //public virtual int DataAssetId
        //{
        //    get { return _dataAssetId; }
        //    set { _dataAssetId = value; }
        //}
        public virtual DateTime ExpirationTime
        {
            get { return _expirationTime; }
            set { _expirationTime = value; }
        }
        public virtual DateTime StartTime
        {
            get { return _startTime; }
            set { _startTime = value; }
        }
        public virtual string CreateUser
        {
            get { return _createUser; }
            set { _createUser = value; }
        }
        public virtual string Message
        {
            get { return _message; }
            set { _message = value; }
        }
        public virtual int NotificationId
        {
            get { return _notificationId; }
            set { _notificationId = value; }
        }
        public virtual string DisplayMessage
        {
            get
            {
                switch (_messageSeverityTag)
                {
                    case "danger":
                        return $"<strong class=\"alertHeading\">Alert!</strong> {_message}";
                    case "warning":
                        return $"<strong class=\"alertHeading\">Warning!</strong> {_message}";
                    case "info":
                        return $"<strong class=\"alertHeading\">Info: </strong> {_message}";
                    default :
                        return _message;
                }
            }
        }
        public virtual string MessageSeverityTag
        {
            get
            {
                if (_messageSeverityTag == null)
                {
                    _messageSeverityTag = Enum.GetName(typeof(NotificationSeverity), this._messageSeverity);
                    return _messageSeverityTag;
                }
                else
                {
                    return _messageSeverityTag;
                }                
            }
        }
        public virtual DataAsset ParentDataAsset
        {
            get { return _parentDataAsset; }
            set { _parentDataAsset = value; }
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
