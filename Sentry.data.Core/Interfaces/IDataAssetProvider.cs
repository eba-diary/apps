using Sentry.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IDataAssetProvider : IWritableDomainContext
    {
        DataAsset GetDataAsset(int id);
        DataAsset GetDataAsset(string assetname);
        IList<DataAsset> GetDataAssets();
        List<AssetNotifications> GetNotificationsByDataAssetId(int id);
        IEnumerable<AssetNotifications> GetAllNotifications();
        AssetNotifications GetNotificationByID(int id);
    }
}
