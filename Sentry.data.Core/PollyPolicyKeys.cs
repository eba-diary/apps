namespace Sentry.data.Core
{
    public static class PollyPolicyKeys
    {
        public static string ApacheLivyProviderAsyncPolicy => nameof(ApacheLivyProviderAsyncPolicy);
        public static string ConfluentConnectorProviderAsyncPolicy => nameof(ConfluentConnectorProviderAsyncPolicy);
        public static string BaseProviderPolicy => nameof(BaseProviderPolicy);
        public static string GoogleAPiProviderPolicy => nameof(GoogleAPiProviderPolicy);
        public static string GenericHttpProviderPolicy => nameof(GenericHttpProviderPolicy);
        public static string FtpProviderPolicy => nameof(FtpProviderPolicy);
    }
}