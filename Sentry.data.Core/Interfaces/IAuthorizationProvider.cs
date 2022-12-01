namespace Sentry.data.Core
{
    public interface IAuthorizationProvider
    {
        string GetOAuthAccessToken(HTTPSSource source);
        string GetOAuthAccessTokenForToken(HTTPSSource source, DataSourceToken token);
    }
}
