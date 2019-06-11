using Sentry.Core;
using System;
using System.Linq;
using System.Collections.Generic;
using Sentry.data.Core.Entities;

namespace Sentry.data.Core
{
    public interface IDatasetContext : IWritableDomainContext
    {
        /** IQueryables **/

        IQueryable<Dataset> Datasets { get; }
        IQueryable<DatasetFileConfig> DatasetFileConfigs { get; }
        IQueryable<SecurityTicket> HpsmTickets { get; }
        IQueryable<Security> Security { get; }
        IQueryable<SecurityPermission> SecurityPermission { get; }
        IQueryable<SecurityTicket> SecurityTicket { get; }
        IQueryable<Permission> Permission { get; }
        IQueryable<DataSourceType> DataSourceTypes { get; }
        IQueryable<DataSource> DataSources { get; }
        IQueryable<DatasetScopeType> DatasetScopeTypes { get; }
        IQueryable<AuthenticationType> AuthTypes { get; }
        IQueryable<EventType> EventTypes { get; }
        IQueryable<Event> Events { get; }
        IQueryable<Status> EventStatus { get; }
        IQueryable<DataElement> DataElements { get; }
        IQueryable<DataElementDetail> DataElementDetails { get; }
        IQueryable<DataObject> DataObjects { get; }
        IQueryable<DatasetFile> DatasetFile { get; }
        IQueryable<FileExtension> FileExtensions { get; }
        IQueryable<Category> Categories { get; }
        IQueryable<BusinessUnit> BusinessUnits { get; }
        IQueryable<DatasetFunction> DatasetFunctions { get; }
        IQueryable<HiveTable> HiveTables { get; }
        IQueryable<LivyCreation> LivySessions { get; }
        IQueryable<MediaTypeExtension> MediaTypeExtensions { get; }
        IQueryable<JobHistory> JobHistory { get; }
        IQueryable<RetrieverJob> Jobs { get; }
        IQueryable<MetadataTag> Tags { get; }
        IQueryable<TagGroup> TagGroups { get; }
        IQueryable<ApplicationConfiguration> ApplicationConfigurations { get; }
        IQueryable<Favorite> Favorites { get; }
        IQueryable<OAuthClaim> OAuthClaims { get; }

        IQueryable<RetrieverJob> RetrieverJob { get; }
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
        int GetLatestDatasetFileIdForDataset(int id);
        int GetLatestDatasetFileIdForDatasetByDatasetFileConfig(int datasetId, int dataFileConfigId, bool isBundled, string targetFileName = null, DataElement schema = null);
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
        int GetNextStorageCDE();
        int GetReportCount();


        /** Favorites **/

        Favorite GetFavorite(int favoriteId);
        List<Favorite> GetFavorites(List<int> favoriteIds);
        
    }

}
