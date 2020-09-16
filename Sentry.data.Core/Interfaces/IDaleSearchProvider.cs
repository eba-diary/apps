namespace Sentry.data.Core
{
    public interface IDaleSearchProvider
    {
        DaleResultDto GetSearchResults(DaleSearchDto dto);
        bool SaveSensitive(string sensitiveBlob);

        DaleContainSensitiveResultDto DoesItemContainSensitive(DaleSearchDto dto);

    }
}
