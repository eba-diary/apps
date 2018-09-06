using Sentry.Core;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sentry.data.Core.Entities.Metadata;
using Sentry.data.Core.Entities;

namespace Sentry.data.Core
{
    public interface IDatasetContext : IWritableDomainContext
    {
        /** IQueryables **/

        IQueryable<Dataset> Datasets { get; }
        IQueryable<DataSourceType> DataSourceTypes { get; }
        IQueryable<DataSource> DataSources { get; }
        IQueryable<AuthenticationType> AuthTypes { get; }
        IQueryable<EventType> EventTypes { get; }
        IQueryable<Event> Events { get; }
        IQueryable<Status> EventStatus { get; }
        IQueryable<DataElement> DataElements { get; }
        IQueryable<DataObject> DataObjects { get; }
        IQueryable<FileExtension> FileExtensions { get; }
        IQueryable<Category> Categories { get; }
        IQueryable<Schema> Schemas { get; }
        IQueryable<HiveTable> HiveTables { get; }
        IQueryable<LivyCreation> LivySessions { get; }
        IQueryable<MediaTypeExtension> MediaTypeExtensions { get; }

        /** Datasets **/

        Dataset GetById(int id);
        int GetDatasetCount();
        Boolean isDatasetNameDuplicate(string datasetName, string category);
        string GetPreviewKey(int id);
        IEnumerable<Dataset> GetDatasetByCategoryID(int id);

        IEnumerable<DatasetScopeType> GetAllDatasetScopeTypes();
        IEnumerable<DatasetFileConfig> getAllDatasetFileConfigs();
        DatasetFileConfig getDatasetFileConfigs(int configId);

        IEnumerable<DatasetFile> GetDatasetFilesForDataset(int datasetId, Func<DatasetFile, bool> where);
        IEnumerable<DatasetFile> GetDatasetFilesForDatasetFileConfig(int configId, Func<DatasetFile, bool> where);
        IEnumerable<DatasetFile> GetDatasetFilesVersions(int datasetId, int dataFileConfigId, string filename);
        int GetLatestDatasetFileIdForDataset(int id);
        IEnumerable<DatasetFile> GetAllDatasetFiles();
        DatasetFile GetDatasetFile(int id);
        int GetLatestDatasetFileIdForDatasetByDatasetFileConfig(int datasetId, int dataFileConfigId, bool isBundled, string targetFileName = null);
        Category GetCategoryById(int id);


        /** Subscriptions and Events **/

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
