using System.ComponentModel.DataAnnotations;

namespace Sentry.data.Web
{
    public class UserSwitchModel
    {
        [Required()]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "User ID must be 6 digits long")]
        public string RequestedUserSwitchId { get; set; }
        public string CurrentRealUserName { get; set; }
        public string CurrentImpersonatedUserName { get; set; }
    }
}
