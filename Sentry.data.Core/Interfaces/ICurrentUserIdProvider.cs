namespace Sentry.data.Core
{
    /// <summary>
    /// Provides methods for getting the current "real" user id and "impersonated" user id.  It is up
    /// to a top-level application layer (such as a web layer or console app) to provide an implementation
    /// of this interface that uses an appropriate data source (such as HttpContext) to retrieve these values.
    /// </summary>
    /// <remarks></remarks>
    public interface ICurrentUserIdProvider
    {
        string GetRealUserId();
        string GetImpersonatedUserId();
    }
}
