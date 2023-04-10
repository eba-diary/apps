namespace Sentry.data.Core
{
    public class ResourceFeatureNotEnabledException : BaseResourceException
    {
        public string FeatureFlagName { get; }

        public ResourceFeatureNotEnabledException(string featureFlagName, string resourceAction) : base(resourceAction, 0)
        {
            FeatureFlagName = featureFlagName;
        }
    }
}