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

        public static List<DataAssetHealth> GetByAssetName(string assetName, IList<AssetSource> assetSource)
        {
            List<DataAssetHealth> dahList = _metadataRespositoryProvider.GetByAssetName(assetName);
            List<DataAssetHealth> outList = new List<DataAssetHealth>();

            foreach(DataAssetHealth dah in dahList)
            {
                if (assetSource.Where(w => w.IsVisiable && w.MetadataRepositorySrcSysName.ToLower() == dah.SourceSystem.ToLower()).Count() > 0)
                {
                    outList.Add(dah);
                }
            }

            return outList;
        }
    }
}
