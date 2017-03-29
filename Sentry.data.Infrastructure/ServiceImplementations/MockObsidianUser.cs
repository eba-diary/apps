using Sentry.Web.Security;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Infrastructure
{
    public class MockObsidianUser : IUser
    {
        public IEnumerable<string> Permissions;

        public string GetFirstLastName()
        {
            return "John Mocker";
        }

        public string GetFirstName()
        {
            return "John";
        }

        public string GetLastName()
        {
            return "Mocker";
        }

        public IList<string> GetPermissions()
        {
            return Permissions.ToList();
        }

        public Boolean HasPermission(string permissionName)
        {
            return Permissions.Contains(permissionName);
        }

        public Boolean IsAuthenticated()
        {
            return true;
        }

        public Boolean IsInGroup(string groupName)
        {
            return true;
        }

        public SentryLoginUserProfileInfo UserProfile { get; set; }
    }
}
