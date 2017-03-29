using System;

namespace Sentry.data.Core
{
    public class ImpersonatedApplicationUser : ApplicationUser
    {
        private IApplicationUser _realUser;
        
        internal ImpersonatedApplicationUser(IApplicationUser realUser, Lazy<DomainUser> appUser, IExtendedUserInfo extendedUserInfo) : base(appUser, extendedUserInfo)
        {
            _realUser = realUser;
        }

        public IApplicationUser RealUser 
        {
            get
            {
                return _realUser;
            }
        }

        //This is an example of an override of behavior so that an impersonated user has a different
        //definition of what it means to have a certain permission.
        public override Boolean CanUserSwitch
        {
            get
            {
                // Check to make sure both the user that is being impersonated
                // and the "real" user have the user switch permission
                return (base.CanUserSwitch && RealUser.CanUserSwitch);
            }
        }
    }
}
