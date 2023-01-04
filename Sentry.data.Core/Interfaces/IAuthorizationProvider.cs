using System;

namespace Sentry.data.Core
{
    public interface IAuthorizationProvider : IDisposable
    {
        string GetOAuthAccessToken(HTTPSSource source, DataSourceToken token);
        string GetTokenAuthenticationToken(HTTPSSource source);
    }
}
