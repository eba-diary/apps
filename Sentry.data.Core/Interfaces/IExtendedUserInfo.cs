using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IExtendedUserInfo
    {
        string UserId { get; }
        Boolean IsInGroup(string groupName);
        IEnumerable<string> Permissions { get; }

        string FirstName { get; }
        string LastName { get; }
        string FamiliarName { get; }
        string EmailAddress { get; }
    }
}
