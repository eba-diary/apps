using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class NotificationService : INotificationService
    {
        private readonly IDatasetContext _domainContext;
        private readonly ISecurityService _securityService;
        private readonly UserService _userService;

        public NotificationService(IDatasetContext domainContext, ISecurityService securityService, UserService userService)
        {
            _domainContext = domainContext;
            _securityService = securityService;
            _userService = userService;
        }


        public bool CanUserModifyNotifications()
        {
            List<DataAsset> dataAssets = _domainContext.DataAsset.ToList();
            IApplicationUser user = _userService.GetCurrentUser();

            foreach(var asset in dataAssets)
            {
                if(_securityService.GetUserSecurity(asset, user).CanModifyNotifications)
                {
                    return true;
                }
            }

            return false;
        }

    }
}
