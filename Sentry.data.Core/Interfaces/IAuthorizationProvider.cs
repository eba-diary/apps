namespace Sentry.data.Core
{
    public interface IAuthorizationProvider
    {
        string GetOAuthAccessToken(HTTPSSource source);
    }
}
