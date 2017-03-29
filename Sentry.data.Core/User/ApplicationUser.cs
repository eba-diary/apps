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

    }
}
