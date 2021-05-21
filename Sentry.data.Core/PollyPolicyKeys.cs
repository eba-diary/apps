namespace Sentry.data.Core
{
    public static class PollyPolicyKeys
    {
        public static string ApacheLivyProviderAsyncPolicy => nameof(ApacheLivyProviderAsyncPolicy);
        public static string BaseProviderPolicy => nameof(BaseProviderPolicy);
    }
}