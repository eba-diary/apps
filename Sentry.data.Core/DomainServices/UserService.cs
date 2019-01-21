using System;
using System.Linq;

namespace Sentry.data.Core
{
    public class UserService : IUserService
    {
        private IExtendedUserInfoProvider _extendedUserInfoProvider;
        private IDataAssetContext _domainContext;
        private ICurrentUserIdProvider _currentUserIdProvider;

        public UserService(IDataAssetContext domainContext, IExtendedUserInfoProvider extendedUserInfoProvider, ICurrentUserIdProvider currentUserIdProvider)
        {
            _domainContext = domainContext;
            _extendedUserInfoProvider = extendedUserInfoProvider;
            _currentUserIdProvider = currentUserIdProvider;
        }

        /// <summary>
        /// Gets an IApplicationUser by Associate ID.  This is commonly used 
        /// </summary>
        /// <param name="associateId"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual IApplicationUser GetByAssociateId(string associateId)
        {
            Lazy<DomainUser> appUser = new Lazy<DomainUser>((() => AddOrGetUser(associateId)));
            return GetBy(appUser, associateId);
        }

        public virtual IApplicationUser GetById(int id)
        {
            Lazy<DomainUser> appUser = new Lazy<DomainUser>((() => _domainContext.GetById<DomainUser>(id)));
            return GetBy(appUser, appUser.Value.AssociateId);
        }

        public virtual IApplicationUser GetByDomainUser(DomainUser domainUser)
        {
            Lazy<DomainUser> wrappedAppUser = new Lazy<DomainUser>((() => domainUser));
            return GetBy(wrappedAppUser, domainUser.AssociateId);
        }

        private IApplicationUser GetBy(Lazy<DomainUser> appUser, string associateId)
        {
            IExtendedUserInfo extendedUserInfo = _extendedUserInfoProvider.GetByUserId(associateId);
            return new ApplicationUser(appUser, extendedUserInfo);
        }

        public virtual IApplicationUser GetCurrentUser()
        {
            string impersonatedId = _currentUserIdProvider.GetImpersonatedUserId();
            string realId = _currentUserIdProvider.GetRealUserId();
            IApplicationUser realUser = GetByAssociateId(realId);

            if (string.IsNullOrEmpty(impersonatedId) || ! realUser.CanUserSwitch)
            {
                return realUser;
            }
            else
            {
                Lazy<DomainUser> impersonatedAppUser = new Lazy<DomainUser>((() => AddOrGetUser(impersonatedId)));
                IExtendedUserInfo impersonatedUserInfo = _extendedUserInfoProvider.GetByUserId(impersonatedId);
                ImpersonatedApplicationUser impersonatedUser = new ImpersonatedApplicationUser(realUser, impersonatedAppUser, impersonatedUserInfo);
                return impersonatedUser;
            }

        }

        public virtual IApplicationUser GetCurrentRealUser()
        {
            string currentUserId = _currentUserIdProvider.GetRealUserId();
            return GetByAssociateId(currentUserId);

        }

        private DomainUser AddOrGetUser(string associateId)
        {
            DomainUser user = _domainContext.Users.FirstOrDefault(((u) => u.AssociateId == associateId));
            if (user == null)
            {
                DomainUser newUser = new DomainUser(associateId);
                _domainContext.Add(newUser);
                return newUser;
            }
            else
            {
                return user;
            }
        }
    }
}
