namespace Sentry.data.Core
{
    public class ResourceForbiddenException : BaseResourceException
    {
        public string UserId { get; }
        public string Permission { get; }

        public ResourceForbiddenException(string userId, string permission, string resourceAction, int resourceId) : base(resourceAction, resourceId)
        {
            UserId = userId;
            Permission = permission;
        }

        public ResourceForbiddenException(string userId, string permission, string resourceAction) : this(userId, permission, resourceAction, 0)
        {
        }
    }
}
