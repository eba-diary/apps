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
                return _extendedUserInfo.Permissions.Contains(GlobalConstants.PermissionCodes.USE_APP);
            }
        }
        public virtual Boolean CanUserSwitch
        {
            get
            {
                return _extendedUserInfo.Permissions.Contains(GlobalConstants.PermissionCodes.USER_SWITCH);
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
                return _extendedUserInfo.Permissions.Contains(GlobalConstants.PermissionCodes.DATASET_VIEW);
            }
        }
        public virtual Boolean CanModifyDataset
        {
            get
            {
                return _extendedUserInfo.Permissions.Contains(GlobalConstants.PermissionCodes.DATASET_MODIFY);
            }
        }
        public virtual Boolean CanViewDataAsset
        {
            get
            {
                return _extendedUserInfo.Permissions.Contains(GlobalConstants.PermissionCodes.DATA_ASSET_VIEW);
            }
        }
        public virtual Boolean CanManageAssetAlerts
        {
            get
            {
                return _extendedUserInfo.Permissions.Contains(GlobalConstants.PermissionCodes.DATA_ASSET_MODIFY);
            }
        }
        public virtual Boolean CanViewReports
        {
            get
            {
                return _extendedUserInfo.Permissions.Contains(GlobalConstants.PermissionCodes.REPORT_VIEW);
            }
        }
        public virtual Boolean CanManageReports
        {
            get
            {
                return _extendedUserInfo.Permissions.Contains(GlobalConstants.PermissionCodes.REPORT_MODIFY);
            }
        }

        public virtual bool IsAdmin
        {
            get
            {
                return _extendedUserInfo.Permissions.Contains(GlobalConstants.PermissionCodes.ADMIN_USER);
            }
        }

        public virtual bool CanViewSensitiveDataInventory
        {
            get
            {
                return _extendedUserInfo.Permissions.Contains(GlobalConstants.PermissionCodes.DALE_SENSITIVE_VIEW);
            }
        }

        public virtual bool CanViewDataInventory
        {
            get
            {
                return _extendedUserInfo.Permissions.Contains(GlobalConstants.PermissionCodes.DALE_VIEW);
            }
        }

        public virtual bool CanEditSensitiveDataInventory
        {
            get
            {
                return _extendedUserInfo.Permissions.Contains(GlobalConstants.PermissionCodes.DALE_SENSITIVE_EDIT);
            }
        }

        public virtual bool CanEditOwnerVerifiedDataInventory
        {
            get
            {
                return _extendedUserInfo.Permissions.Contains(GlobalConstants.PermissionCodes.DALE_OWNER_VERIFIED_EDIT);
            }
        }

    }
}
