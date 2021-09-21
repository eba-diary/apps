using Sentry.data.Core;
using Sentry.FeatureFlags;
using Sentry.FeatureFlags.Conditional;
using Sentry.FeatureFlags.Repo;

namespace Sentry.data.Infrastructure.FeatureFlags
{
    public class AdminUserBaseConditionalStrategy : IConditionalStrategy<bool,string>
    {
        private readonly ISecurityService _securityService;
        private readonly UserService _userService;
        private readonly IReadableFeatureRepository _repo;

        public AdminUserBaseConditionalStrategy(ISecurityService securityService, UserService userService,
                                                IReadableFeatureRepository repo)
        {
            _securityService = securityService;
            _userService = userService;
            _repo = repo;
        }

        public bool GetValue(IBaseFeatureFlag<bool> feature, string userId)
        {
            
            //if feature = false, feature will be false for everyone
            //if feature = true, feature will be true for DSC admins and false for all other users                        
            return (_repo.FindByKey(feature.Key).Value.ToUpper() == "TRUE" && _securityService.GetUserSecurity(null, _userService.GetByAssociateId(userId)).ShowAdminControls);
        }
    }
}
