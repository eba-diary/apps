using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class MetadataRepositoryService
    {
        private static IMetadataRepositoryProvider _metadataRespositoryProvider;

        public MetadataRepositoryService(IMetadataRepositoryProvider metadataRepositoryProvider)
        {
            _metadataRespositoryProvider = metadataRepositoryProvider;
        }

        public static List<DataAssetHealth> GetByAssetName(string assetName)
        {
            return _metadataRespositoryProvider.GetByAssetName(assetName);
        }
    }
}
