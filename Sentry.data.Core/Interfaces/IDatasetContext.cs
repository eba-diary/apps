using Sentry.Core;
using Sentry.data.Core.Entities;
using Sentry.data.Core.Entities.DataProcessing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public interface IDatasetContext : IWritableDomainContext
    {
        /** IQueryables **/

        IQueryable<Dataset> Datasets { get; }
        IQueryable<Asset> Assets { get; }
        IQueryable<DataAsset> DataAsset { get; }
        IQueryable<Notification> Notification { get; }
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
        IQueryable<DatasetFile> DatasetFileStatusActive { get; }
        IQueryable<DatasetFile> DatasetFileStatusAll { get; }

        IQueryable<FileExtension> FileExtensions { get; }
        IQueryable<Category> Categories { get; }
        IQueryable<BusinessUnit> BusinessUnits { get; }
        IQueryable<DatasetFunction> DatasetFunctions { get; }
        IQueryable<HiveTable> HiveTables { get; }
        IQueryable<LivyCreation> LivySessions { get; }
        IQueryable<MediaTypeExtension> MediaTypeExtensions { get; }
        IQueryable<JobHistory> JobHistory { get; }
        IQueryable<Submission> Submission { get; }
        IQueryable<RetrieverJob> Jobs { get; }
        IQueryable<MetadataTag> Tags { get; }
        IQueryable<TagGroup> TagGroups { get; }
        IQueryable<ApplicationConfiguration> ApplicationConfigurations { get; }
        IQueryable<Favorite> Favorites { get; }
        IQueryable<UserFavorite> UserFavorites { get; }
        IQueryable<SupportLink> SupportLinks { get; }
        IQueryable<BusinessAreaTileRow> BusinessAreaTileRows { get; }
        IQueryable<BusinessArea> BusinessAreas { get; }
        IQueryable<OAuthClaim> OAuthClaims { get; }
        IQueryable<RetrieverJob> RetrieverJob { get; }
        IQueryable<DatasetFileParquet> DatasetFileParquet { get; }
        IQueryable<DatasetFileReply> DatasetFileReply { get; }
        IQueryable<Schema> Schema { get; }
        IQueryable<FileSchema> FileSchema { get; }
        IQueryable<SchemaRevision> SchemaRevision { get; }
        IQueryable<BaseField> BaseFields { get; }
        IQueryable<DataFlow> DataFlow { get; }
        IQueryable<DataFlowStep> DataFlowStep { get; }
        IQueryable<S3DropAction> S3DropAction { get; }
        IQueryable<ProducerS3DropAction> ProducerS3DropAction { get; }
        IQueryable<SchemaLoadAction> SchemaLoadAction { get; }
        IQueryable<RawStorageAction> RawStorageAction { get; }
        IQueryable<QueryStorageAction> QueryStorageAction { get; }
        IQueryable<ConvertToParquetAction> ConvertToParquetAction { get; }
        IQueryable<UncompressZipAction> UncompressZipAction { get; }
        IQueryable<UncompressGzipAction> UncompressGzipAction { get; }
        IQueryable<SchemaMapAction> SchemaMapAction { get; }
        IQueryable<GoogleApiAction> GoogleApiAction { get; }
        IQueryable<GoogleBigQueryApiAction> GoogleBigQueryApiAction { get; }
        IQueryable<GoogleSearchConsoleApiAction> GoogleSearchConsoleApiAction { get; }
        IQueryable<FixedWidthAction> FixedWidthAction { get; }
        IQueryable<XMLAction> XMLAction { get; }
        IQueryable<JsonFlatteningAction> JsonFlatteningAction { get; }
        IQueryable<SchemaMap> SchemaMap { get; }
        IQueryable<ClaimIQAction> ClaimIQAction { get; }
        IQueryable<SavedSearch> SavedSearches { get; }
        /** Datasets **/

        Dataset GetById(int id);
        int GetDatasetCount();
        Boolean isDatasetNameDuplicate(string datasetName, string category);
        IEnumerable<Dataset> GetDatasetByCategoryID(int id);

        IEnumerable<DatasetScopeType> GetAllDatasetScopeTypes();
        IEnumerable<DatasetFileConfig> getAllDatasetFileConfigs();
        DatasetFileConfig getDatasetFileConfigs(int configId);

        IEnumerable<DatasetFile> GetDatasetFilesForDataset(int datasetId, Func<DatasetFile, bool> where);
        IEnumerable<DatasetFile> GetDatasetFilesForDatasetFileConfig(int configId, Func<DatasetFile, bool> where);
        int GetLatestDatasetFileIdForDataset(int id);
        int GetLatestDatasetFileIdForDatasetByDatasetFileConfig(int datasetId, int dataFileConfigId, bool isBundled, string targetFileName = null, SchemaRevision schema = null);
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
        
        List<BusinessAreaSubscription> GetAllUserSubscriptionsByEventTypeGroup(string SentryOwnerName,EventTypeGroup group);

        bool IsUserSubscribedToDataAsset(string SentryOwnerName, int dataAssetID);
        List<DataAssetSubscription> GetAllUserSubscriptionsForDataAsset(string SentryOwnerName, int dataAssetID);
        List<DatasetSubscription> GetSubscriptionsForDataset(int datasetID);
        List<DataAssetSubscription> GetSubscriptionsForDataAsset(int dataAssetID);
        List<DatasetSubscription> GetAllSubscriptions();
        List<Subscription> GetAllSubscriptionsForReal();

        List<Event> EventsSince(DateTime time, Boolean IsProcessed);
        int GetNextStorageCDE();
        string GetNextDataFlowStorageCDE();
        int GetReportCount();


        /** Favorites **/

        Favorite GetFavorite(int favoriteId);
        List<Favorite> GetFavorites(List<int> favoriteIds);
        
    }

}
