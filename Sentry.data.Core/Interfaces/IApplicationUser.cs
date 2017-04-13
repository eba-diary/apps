using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    /// <summary>
    /// The IApplicationUser represents one user of the application.  This may be the "current user" or
    /// any other user.  It acts as an aggregate of all information we care about pertaining to the user,
    /// no matter the source.  Typical sources include our own domain, HR, and Obsidian.  It is essential
    /// that an instance of IApplicationUser is NOT cached or persisted in any way, as it may contain "hooks" to
    /// a particular domain context, for example.  Instead, use the UserService to get a new instance of
    /// an IApplicationUser whenever one is needed.  They are lightweight to create, and external information sources
    /// should be caching data anyway, so no need to cache the IApplicationUser.
    /// </summary>
    /// <remarks></remarks>
    public interface IApplicationUser
    {

        string AssociateId { get; }

        //Simple properties that comes from external data sources
        string EmailAddress { get; }

        //Security/Permission checks
        //###  BEGIN Sentry.Data  A### - Code below is Sentry.Data-specific
        Boolean CanApproveItems { get; }
        Boolean CanDwnldSenstive { get; }
        Boolean CanDwnldNonSensitive { get; }
        Boolean CanEditDataset { get; }
        Boolean CanUpload { get; }
        //###  END Sentry.Data  ### - Code above is Sentry.Data-specific
        Boolean CanUseApp { get; }
        Boolean CanUserSwitch { get; }
        IEnumerable<string> Permissions { get; }

        //Calculated values - may come from external data sources and/or our domain User object
        string DisplayName { get; }

        DomainUser DomainUser { get; }
    }
}
