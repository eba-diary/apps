using Sentry.Core;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sentry.data.Core.Entities.Metadata;

namespace Sentry.data.Core
{
    public interface IDatasetContext : IWritableDomainContext
    {
        //###  BEGIN Sentry.Data  A### - Code below is Sentry.Data-specific
        IQueryable<Dataset> Datasets { get; }
        //IQueryable<Category> DatasetMetadata { get; }


        IQueryable<DataSourceType> DataSourceTypes { get; }

        IQueryable<DataSource> DataSources { get; }

        IQueryable<AuthenticationType> AuthTypes { get; }

        IQueryable<EventType> EventTypes { get; }

        IQueryable<Status> EventStatus { get; }

        IQueryable<DataElement> DataElements { get; }

        IQueryable<DataObject> DataObjects { get; }

        IQueryable<DatasetScopeType> DatasetScopeTypes { get; }

        List<String> BusinessTerms(string dataElementCode, int? DataAsset_ID);

        Lineage Description(int DataAsset_ID, string DataObject_NME, string DataObjectField_NME);

        List<Lineage> Lineage(string dataElementCode, List<string> dataObjectFieldDetailTypes, int? DataAsset_ID, String DataElement_NME = "", String DataObject_NME = "", String DataObjectField_NME = "");

        Dataset GetById(int id);

        int GetDatasetCount();

        int GetCategoryDatasetCount(Category cat);

        int GetMaxId();

        Dataset GetByS3Key(string S3key);

        IEnumerable<String> GetCategoryList();

        IEnumerable<String> GetSentryOwnerList();

        IQueryable<Category> Categories { get; }

        IEnumerable<DatasetFrequency> GetDatasetFrequencies();
        
        void DeleteAllData();

        Boolean s3KeyDuplicate(string s3key);

        /// <summary>
        /// Checks for duplicate s3Key across all datasetfiles for specified dataset
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="s3key"></param>
        /// <returns>asdfasdf</returns>
        Boolean s3KeyDuplicate(int datasetId, string s3key);

        Boolean isDatasetNameDuplicate(string datasetName, string category);

        string GetPreviewKey(int id);

        IEnumerable<DatasetFile> GetDatasetFilesForDataset(int id, Func<DatasetFile, bool> where);

        IEnumerable<DatasetFile> GetDatasetFilesVersions(int datasetId, int dataFileConfigId, string filename);

        int GetLatestDatasetFileIdForDataset(int id);

        IEnumerable<DatasetFile> GetAllDatasetFiles();

        DatasetFile GetDatasetFile(int id);

        IEnumerable<Dataset> GetDatasetByCategoryID(int id);

        Category GetCategoryById(int id);

        IEnumerable<DatasetScopeType> GetAllDatasetScopeTypes();

        DatasetScopeType GetDatasetScopeById(int id);

        //###  END Sentry.Data  ### - Code above is Sentry.Data-specific

        Category GetCategoryByName(string name);

        IEnumerable<DatasetFileConfig> getAllDatasetFileConfigs();

        int GetLatestDatasetFileIdForDatasetByDatasetFileConfig(int datasetId, int dataFileConfigId, bool isBundled);
        int GetLatestBundleFileIdForDatasetByDatasetFileConfig(int datasetId, int dataFileConfigId);
        int GetLatestDatasetFileIdForDatasetByDatasetFileConfig(int datasetId, int dataFileConfigId, string targetFileName, bool isBunled);

        DatasetFileConfig getDatasetFileConfigs(int configId);
        DatasetFileConfig getDatasetDefaultConfig(int datasetId);
        IEnumerable<AssetNotifications> GetAssetNotificationsByDataAssetId(int id);
        IEnumerable<AssetNotifications> GetAllAssetNotifications();
        AssetNotifications GetAssetNotificationByID(int id);
        IList<DataAsset> GetDataAssets();
        DataAsset GetDataAsset(int id);
        DataAsset GetDataAsset(string assetname);


        Interval GetInterval(string description);

        List<Interval> GetAllIntervals();

        Interval GetInterval(int id);

        Status GetStatus(string description);

        Status GetStatus(int id);

        List<Status> GetAllStatuses();

        List<Event> GetEvents(string reason);

        Event GetEvent(int id);

        List<Event> GetEventsStartedByUser(string SentryOwnerName);

        bool IsUserSubscribedToDataset(string SentryOwnerName, int datasetID);

        List<DatasetSubscription> GetAllUserSubscriptionsForDataset(string SentryOwnerName, int datasetID);

        bool IsUserSubscribedToDataAsset(string SentryOwnerName, int dataAssetID);

        List<DataAssetSubscription> GetAllUserSubscriptionsForDataAsset(string SentryOwnerName, int dataAssetID);

        List<DatasetSubscription> GetSubscriptionsForDataset(int datasetID);

        List<DataAssetSubscription> GetSubscriptionsForDataAsset(int dataAssetID);

        List<DatasetSubscription> GetAllSubscriptions();

        List<Event> EventsSince(DateTime time, Boolean IsProcessed);
    }

}
