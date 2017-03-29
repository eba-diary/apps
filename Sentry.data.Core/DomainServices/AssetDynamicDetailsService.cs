using System;

namespace Sentry.data.Core
{
    public class AssetDynamicDetailsService
    {
        private static IAssetDynamicDetailsProvider _dynamicDetailsProvider;

        public AssetDynamicDetailsService(IAssetDynamicDetailsProvider dynamicDetailsProvider)
        {
            _dynamicDetailsProvider = dynamicDetailsProvider;
        }

        public static AssetDynamicDetails GetByAssetId(int assetId)
        {
            return _dynamicDetailsProvider.GetByAssetId(assetId);
        }
    }
}
