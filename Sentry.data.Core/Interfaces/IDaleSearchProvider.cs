namespace Sentry.data.Core
{
    public interface IDaleSearchProvider
    {
        DaleResultDto GetSearchResults(DaleSearchDto dto);
        FilterSearchDto GetSearchFilters(DaleSearchDto dto);
        bool SaveSensitive(string sensitiveBlob);
        DaleContainSensitiveResultDto DoesItemContainSensitive(DaleSearchDto dto);
        DaleCategoryResultDto GetCategoriesByAsset(string search);
    }
}
