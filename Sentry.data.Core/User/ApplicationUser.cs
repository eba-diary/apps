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
        private Lazy<DomainUser> _domainUser;
        private IExtendedUserInfo _extendedUserInfo;
        
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

        //###  BEGIN Sentry.Data  A### - Code below is Sentry.Data-specific
        public virtual Boolean CanApproveItems
        {
            get
            {
                return _extendedUserInfo.Permissions.Contains(PermissionNames.ApproveItems);
            }
        }

        public virtual Boolean CanDwnldSenstive
        {
            get
            {
                //return true;
                return _extendedUserInfo.Permissions.Contains(PermissionNames.DwnldSensitive);
            }
        }

        public virtual Boolean CanEditDataset
        {
            get
            {
                return _extendedUserInfo.Permissions.Contains(PermissionNames.DatasetEdit);
            }
        }

        public virtual Boolean CanUpload
        {
            get
            {
                return _extendedUserInfo.Permissions.Contains(PermissionNames.Upload);
            }
        }

        public virtual Boolean CanDwnldNonSensitive
        {
            get
            {
                return _extendedUserInfo.Permissions.Contains(PermissionNames.DwnldNonSensitive);
            }
        }

        //###  END Sentry.Data  ### - Code above is Sentry.Data-specific

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

        public virtual Boolean CanQueryTool
        {
            get
            {
                return _extendedUserInfo.Permissions.Contains(PermissionNames.QueryToolUser);
            }
        }

        public virtual Boolean CanQueryToolPowerUser
        {
            get
            {
                return _extendedUserInfo.Permissions.Contains(PermissionNames.QueryToolPowerUser);
            }
        }

        public virtual Boolean CanQueryToolAdmin
        {
            get
            {
                return _extendedUserInfo.Permissions.Contains(PermissionNames.QueryToolAdmin);
            }
        }



        public virtual Boolean CanManageConfigs
        {
            get
            {
                return _extendedUserInfo.Permissions.Contains(PermissionNames.ManageDataFileConfigs);
            }
        }

        public virtual Boolean CanManageAssetAlerts
        {
            get
            {
                return _extendedUserInfo.Permissions.Contains(PermissionNames.ManageAssetNotifications);
            }
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

        public virtual Boolean CanViewDataAsset
        {
            get
            {
                return _extendedUserInfo.Permissions.Contains(PermissionNames.DatasetView);
            }
        }

    }
}
