using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    /// <summary>
    /// An implementation of IApplicationUser that represents an "actual" or "real" user (as opposed to 
    /// a user that is being impersonated in a "user switch" scenario)
    /// </summary>
    /// <remarks></remarks>
    public class ApplicationUser : IApplicationUser
    {
        private readonly Lazy<DomainUser> _domainUser;
        private readonly IExtendedUserInfo _extendedUserInfo;
        
        internal ApplicationUser(Lazy<DomainUser> domainUser, IExtendedUserInfo extendedUserInfo)
        {
            _domainUser = domainUser;
            _extendedUserInfo = extendedUserInfo;
        }

        public virtual string EmailAddress
        {
            get
            {
                return _extendedUserInfo.EmailAddress;
            }
        }

        public virtual Boolean CanUseApp
        {
            get
            {
                return _extendedUserInfo.Permissions.Contains(PermissionNames.UseApp);
            }
        }

        public virtual Boolean CanUserSwitch
        {
            get
            {
                return _extendedUserInfo.Permissions.Contains(PermissionNames.UserSwitch);
            }
        }


        public virtual Boolean CanManageAssetAlerts
        {
            get
            {
                return _extendedUserInfo.Permissions.Contains(PermissionNames.ManageAssetNotifications);
            }
        }

        public virtual Boolean AdminUser
        {
            get
            {
                return _extendedUserInfo.Permissions.Contains(PermissionNames.AdminUser);
            }
        }

        public virtual bool IsInGroup(string group)
        {
            return _extendedUserInfo.IsInGroup(group);
        }

        public DomainUser DomainUser
        {
            get
            {
                return _domainUser.Value;
            }
        }

        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(_extendedUserInfo.FamiliarName))
                {
                    return _extendedUserInfo.FamiliarName + " " + _extendedUserInfo.LastName;
                }
                else
                {
                    return _extendedUserInfo.FirstName + " " + _extendedUserInfo.LastName;
                }
            }
        }

        public string AssociateId
        {
            get
            {
                return _extendedUserInfo.UserId;
            }
        }

        public virtual IEnumerable<string> Permissions
        {
            get
            {
                return _extendedUserInfo.Permissions;
            }
        }
        public virtual Boolean CanViewDataset
        {
            get
            {
                return _extendedUserInfo.Permissions.Contains(PermissionNames.DatasetView);
            }
        }

        public virtual Boolean CanModifyDataset
        {
            get
            {
                return _extendedUserInfo.Permissions.Contains(PermissionNames.DatasetEdit);
            }
        }

        public virtual Boolean CanViewDataAsset
        {
            get
            {
                return _extendedUserInfo.Permissions.Contains(PermissionNames.DataAssetView);
            }
        }

        public virtual Boolean CanViewReports
        {
            get
            {
                return _extendedUserInfo.Permissions.Contains(PermissionNames.ReportView);
            }
        }

        public virtual Boolean CanManageReports
        {
            get
            {
                return _extendedUserInfo.Permissions.Contains(PermissionNames.ManageReports);
            }
        }

    }
}
