using Sentry.data.Core;
using Sentry.Web.CachedObsidianUserProvider;
using System;

namespace Sentry.data.Infrastructure
{
    /// <summary>
    /// An implementation of IExtendedUserInfoProvider that calls out to Obsidian and HR to get data.
    /// </summary>
    /// <remarks></remarks>
    public class ExtendedUserInfoProvider : IExtendedUserInfoProvider
    {
        private IObsidianUserProvider _obsidianUserProvider;
        private IAssociateInfoProvider _associateInfoProvider;

        public ExtendedUserInfoProvider(IAssociateInfoProvider associateInfoProvider, IObsidianUserProvider obsidianService)
        {
            _associateInfoProvider = associateInfoProvider;
            _obsidianUserProvider = obsidianService;
        }

        public IExtendedUserInfo GetByUserId(string userId)
        {
            // Note the use of "Lazy" to ensure that we don't immediately go out to get this data, but only
            // get it when it's first accessed inside the ExtendedUserInfo class.
            Lazy<Sentry.Web.Security.IUser> obsidianUser = new Lazy<Sentry.Web.Security.IUser>(() => _obsidianUserProvider.GetObsidianUser(userId), System.Threading.LazyThreadSafetyMode.PublicationOnly);
            Lazy<Sentry.Associates.Associate> associateInfo = new Lazy<Sentry.Associates.Associate>(() => _associateInfoProvider.GetAssociateInfo(userId), System.Threading.LazyThreadSafetyMode.PublicationOnly);
            return new ExtendedUserInfo(userId, obsidianUser, associateInfo);
        }
    }
}
