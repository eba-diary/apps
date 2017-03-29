using System.Collections.Generic;

namespace Sentry.data.Infrastructure
{
    public class MockObsidianUserProvider : Sentry.Web.CachedObsidianUserProvider.IObsidianUserProvider
    {
        public Sentry.Web.Security.IUser GetObsidianUser(string id)
        {
            MockObsidianUser obsUser = new MockObsidianUser();

            List<string> perms = new List<string>();
            if (id == "069301" || id == "067664")
            {
                perms.Add(Sentry.data.Core.PermissionNames.UseApp);
                perms.Add(Sentry.data.Core.PermissionNames.AddItems);
                perms.Add(Sentry.data.Core.PermissionNames.UserSwitch );
            }
            if (id == "069301")
            {
                perms.Add(Sentry.data.Core.PermissionNames.ApproveItems);
            }
            obsUser.Permissions = perms;
        return obsUser;
        }
    }
}
