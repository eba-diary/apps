namespace Sentry.data.Core
{
    public interface IExtendedUserInfoProvider
    {
        IExtendedUserInfo GetByUserId(string userId);
    }
}
