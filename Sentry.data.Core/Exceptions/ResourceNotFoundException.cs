namespace Sentry.data.Core
{
    public class ResourceNotFoundException : BaseResourceException
    {
        public ResourceNotFoundException(string resourceAction, int resourceId) : base(resourceAction, resourceId) { }
    }
}
