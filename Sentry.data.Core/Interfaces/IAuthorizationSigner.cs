namespace Sentry.data.Core
{
    public interface IAuthorizationSigner
    {
        string SignOAuthToken(string claims, string privateKey);
    }
}
