namespace Sentry.data.Core
{
    public class ResourceFeatureDisabledException : BaseResourceException
    {
        public string FeatureFlagName { get; }

        public ResourceFeatureDisabledException(string featureFlagName, string resourceAction) : base(resourceAction, 0)
        {
            FeatureFlagName = featureFlagName;
        }
    }
}