using Sentry.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public interface IDataAssetContext : IWritableDomainContext
    {
        IQueryable<DomainUser> Users { get; }
        IQueryable<DataAsset> DataAssets { get; }


        /** Data Assets **/

        IList<DataAsset> GetDataAssets();
        DataAsset GetDataAsset(int id);
        DataAsset GetDataAsset(string assetname);

        /** Data Asset Notifications **/

        IEnumerable<AssetNotifications> GetAssetNotificationsByDataAssetId(int id);
        IEnumerable<AssetNotifications> GetAllAssetNotifications();
        AssetNotifications GetAssetNotificationByID(int id);

        /** Lineage **/

        List<String> BusinessTerms(string dataElementCode, int? DataAsset_ID, String DataElement_NME = "", String DataObject_NME = "", String DataObjectField_NME = "", String Line_CDE = "");
        List<String> ConsumptionLayers(string dataElementCode, int? DataAsset_ID, String DataElement_NME = "", String DataObject_NME = "", String DataObjectField_NME = "", String Line_CDE = "");
        List<String> LineageTables(string dataElementCode, int? DataAsset_ID, String DataElement_NME = "", String DataObject_NME = "", String DataObjectField_NME = "", String Line_CDE = "");
        Lineage Description(int? DataAsset_ID, string DataObject_NME, string DataObjectField_NME, String Line_CDE = "");
        List<String> BusinessTermDescription(string dataElementCode, int? DataAsset_ID, string DataObjectField_NME, String Line_CDE = "");
        List<LineageCreation> Lineage(string dataElementCode, int? DataAsset_ID, String DataElement_NME = "", String DataObject_NME = "", String DataObjectField_NME = "", String Line_CDE = "");
    }

}
