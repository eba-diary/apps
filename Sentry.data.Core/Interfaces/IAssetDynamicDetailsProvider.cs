
namespace Sentry.data.Core
{
    public interface IAssetDynamicDetailsProvider
    {
        AssetDynamicDetails GetByAssetId(int assetId);
    }
}
