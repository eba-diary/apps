﻿using NHibernate;
using NHibernate.Linq;
using Sentry.data.Core;
using Sentry.data.Core.Entities;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.NHibernate;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace Sentry.data.Infrastructure
{

    /// <summary>
    /// Provides common code between projects
    /// </summary>
    /// 
    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> True<T>() { return f => true; }
        public static Expression<Func<T, bool>> False<T>() { return f => false; }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
                                                            Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                  (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
                                                             Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                  (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }

    
    public class DatasetContext : NHWritableDomainContext, IDatasetContext
    {
        public DatasetContext(ISession session) : base(session)
        {
            NHQueryableExtensionProvider.RegisterQueryableExtensionsProvider<DatasetContext>();
        }


        public IQueryable<EventType> EventTypes
        {
            get
            {
                return Query<EventType>();
            }
        }

        public IQueryable<DataSourceType> DataSourceTypes
        {
            get
            {
                return Query<DataSourceType>().Cacheable();
            }
        }

        public IQueryable<DataSource> DataSources
        {
            get
            {
                return Query<DataSource>().Cacheable();
            }
        }

        public IQueryable<DatasetFile> DatasetFile
        {
            get
            {
                return Query<DatasetFile>();
            }
        }

        public IQueryable<DatasetScopeType> DatasetScopeTypes
        {
            get
            {
                return Query<DatasetScopeType>().Cacheable();
            }
        }

        public IQueryable<AuthenticationType> AuthTypes
        {
            get
            {
                IQueryable<AuthenticationType> qresult = Query<AuthenticationType>();
                return qresult;
            }
        }

        public IQueryable<Event> Events
        {
            get
            {
                return Query<Event>();
            }
        }

        public IQueryable<Status> EventStatus
        {
            get
            {
                return Query<Status>().Cacheable();
            }
        }

        public IQueryable<Dataset> Datasets
        {
            get
            {
                return Query<Dataset>();
            }
        }

        public IQueryable<DataAsset> DataAsset
        {
            get
            {
                return Query<DataAsset>();
            }
        }

        public IQueryable<Notification> Notification
        {
            get
            {
                return Query<Notification>();
            }
        }

        public IQueryable<DatasetFileConfig> DatasetFileConfigs
        {
            get
            {
                return Query<DatasetFileConfig>();
            }
        }

        public IQueryable<SecurityTicket> HpsmTickets
        {
            get
            {
                return Query<SecurityTicket>();
            }
        }

        public IQueryable<Security> Security
        {
            get
            {
                return Query<Security>();
            }
        }

        public IQueryable<SecurityPermission> SecurityPermission
        {
            get
            {
                return Query<SecurityPermission>();
            }
        }

        public IQueryable<SecurityTicket> SecurityTicket
        {
            get
            {
                return Query<SecurityTicket>();
            }
        }

        public IQueryable<Permission> Permission
        {
            get
            {
                return Query<Permission>();
            }
        }

        public IQueryable<DataElement> DataElements
        {
            get
            {
                return Query<DataElement>();
            }
        }

        public IQueryable<DataElementDetail> DataElementDetails
        {
            get
            {
                return Query<DataElementDetail>();
            }
        }

        public IQueryable<DataObject> DataObjects
        {
            get
            {
                return Query<DataObject>().Cacheable();
            }
        }

        public IQueryable<FileExtension> FileExtensions
        {
            get
            {
                return Query<FileExtension>().Cacheable();
            }
        }

        public IQueryable<Category> Categories
        {
            get
            {
                return Query<Category>().Cacheable();  //QueryCacheRegion.MediumTerm
            }
        }

        public IQueryable<BusinessUnit> BusinessUnits
        {
            get
            {
                return Query<BusinessUnit>().Cacheable();
            }
        }

        public IQueryable<DatasetFunction> DatasetFunctions
        {
            get
            {
                return Query<DatasetFunction>().Cacheable();
            }
        }

        public IQueryable<HiveTable> HiveTables
        {
            get
            {
                return Query<HiveTable>();  //QueryCacheRegion.MediumTerm
            }
        }

        public IQueryable<LivyCreation> LivySessions
        {
            get
            {
                return Query<LivyCreation>();  //QueryCacheRegion.MediumTerm
            }
        }

        public IQueryable<MediaTypeExtension> MediaTypeExtensions
        {
            get
            {
                return Query<MediaTypeExtension>();  //QueryCacheRegion.MediumTerm
            }
        }

        public IQueryable<RetrieverJob> Jobs
        {
            get
            {
                return Query<RetrieverJob>();
            }
        }
        public IQueryable<DatasetFileParquet> DatasetFileParquet
        {
            get
            {
                return Query<DatasetFileParquet>();
            }
        }
        public IQueryable<DatasetFileReply> DatasetFileReply
        {
            get
            {
                return Query<DatasetFileReply>();
            }
        }

        public IQueryable<JobHistory> JobHistory
        {
            get
            {
                return Query<JobHistory>();
            }
        }

        public IQueryable<MetadataTag> Tags
        {
            get
            {
                return Query<MetadataTag>().Cacheable();
            }
        }

        public IQueryable<TagGroup> TagGroups
        {
            get
            {
                return Query<TagGroup>();
            }
        }

        public IQueryable<RetrieverJob> RetrieverJob
        {
            get
            {
                return Query<RetrieverJob>();
            }
        }

        public IQueryable<ApplicationConfiguration> ApplicationConfigurations
        {
            get
            {
                return Query<ApplicationConfiguration>();
            }
        }

        public IQueryable<Favorite> Favorites
        {
            get
            {
                return Query<Favorite>();
            }
        }

        public IQueryable<BusinessAreaTileRow> BusinessAreaTileRows
        {
            get
            {
                return Query<BusinessAreaTileRow>().Cacheable();
            }
        }

        public IQueryable<BusinessArea> BusinessAreas
        {
            get
            {
                return Query<BusinessArea>().Cacheable();
            }
        }

        public IQueryable<OAuthClaim> OAuthClaims
        {
            get
            {
                return Query<OAuthClaim>();
            }
        }

        public IQueryable<Schema> Schema
        {
            get
            {
                return Query<Schema>();
            }
        }

        public IQueryable<FileSchema> FileSchema
        {
            get
            {
                return Query<FileSchema>();
            }
        }

        public IQueryable<SchemaRevision> SchemaRevision
        {
            get
            {
                return Query<SchemaRevision>();
            }
        }

        public IQueryable<DataFlow> DataFlow
        {
            get
            {
                return Query<DataFlow>();
            }
        }

        public IQueryable<DataFlowStep> DataFlowStep
        {
            get
            {
                return Query<DataFlowStep>();
            }
        }

        public IQueryable<S3DropAction> S3DropAction
        {
            get
            {
                return Query<S3DropAction>();
            }
        }

        public IQueryable<SchemaLoadAction> SchemaLoadAction
        {
            get
            {
                return Query<SchemaLoadAction>();
            }
        }

        public IQueryable<SchemaMapAction> SchemaMapAction
        {
            get
            {
                return Query<SchemaMapAction>();
            }
        }

        public IQueryable<RawStorageAction> RawStorageAction
        {
            get
            {
                return Query<RawStorageAction>();
            }
        }

        public IQueryable<QueryStorageAction> QueryStorageAction
        {
            get
            {
                return Query<QueryStorageAction>();
            }
        }

        public IQueryable<ConvertToParquetAction> ConvertToParquetAction
        {
            get
            {
                return Query<ConvertToParquetAction>();
            }
        }

        public IQueryable<UncompressZipAction> UncompressZipAction
        {
            get
            {
                return Query<UncompressZipAction>();
            }
        }

        public IQueryable<SchemaMap> SchemaMap
        {
            get
            {
                return Query<SchemaMap>();
            }
        }

        public IEnumerable<Dataset> GetDatasetByCategoryID(int id)
        {
            return Query<Dataset>().Where(w => w.DatasetCategories.Any(y=> y.Id == id)).Where(x => x.CanDisplay).AsEnumerable();
        }

        public Category GetCategoryById(int id)
        {
            return Query<Category>().Where(w => w.Id == id).FirstOrDefault();
        }

        public int GetDatasetCount()
        {
            return Query<Dataset>().Count(x => x.CanDisplay && x.DatasetType == GlobalConstants.DataEntityCodes.DATASET);
        }

        public Dataset GetById(int id)
        {
            Dataset ds = Query<Dataset>().Where((x) => x.DatasetId == id && x.CanDisplay).FirstOrDefault();
            return ds;
        }

        public Boolean isDatasetNameDuplicate(string datasetName, string category)
        {
            if (Query<Dataset>().Where(x => x.DatasetName == datasetName && x.DatasetCategories.Any(y=> y.Name == category)).Count() == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public string GetPreviewKey(int id)
        {
            string key = Query<DatasetFile>().Where(x => x.DatasetFileId == id).FirstOrDefault().FileLocation;
            return Configuration.Config.GetSetting("S3PreviewPrefix") + key;
        }
        /// <summary>
        /// Returns all datasetfiles for dataset
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IEnumerable<DatasetFile> GetDatasetFilesForDataset(int datasetId, Func<DatasetFile, bool> where)
        {
            IEnumerable<DatasetFile> list = 
                Query<DatasetFile>().Where
                (
                    x => x.Dataset.DatasetId == datasetId && 
                    x.ParentDatasetFileId == null
                ).Where(where)

                .AsEnumerable();

            return list;
        }

        /// <summary>
        /// Returns all datasetfiles for data file config
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IEnumerable<DatasetFile> GetDatasetFilesForDatasetFileConfig(int configId, Func<DatasetFile, bool> where)
        {
            IEnumerable<DatasetFile> list =
                Query<DatasetFile>().Where
                (
                    x => x.DatasetFileConfig.ConfigId == configId &&
                    x.ParentDatasetFileId == null
                ).Where(where)
                .AsEnumerable();

            return list;
        }

        public int GetLatestDatasetFileIdForDataset(int id)
        {
            int d = Query<DatasetFile>().Where(w => w.Dataset.DatasetId == id && !w.IsBundled).GroupBy(x => x.Dataset.DatasetId).ToList().Select(s => s.OrderByDescending(g => g.CreateDTM).Take(1)).Select(i => i.First().DatasetFileId).FirstOrDefault();
            return d;
        }

        public IEnumerable<DatasetScopeType> GetAllDatasetScopeTypes()
        {
            return Query<DatasetScopeType>().AsEnumerable();
        }

        public DatasetScopeType GetDatasetScopeById(int id)
        {
            return Query<DatasetScopeType>().Where(w => w.ScopeTypeId == id).FirstOrDefault();
        }

        public IEnumerable<DatasetFileConfig> getAllDatasetFileConfigs()
        {
            IEnumerable<DatasetFileConfig> dfcList = Query<DatasetFileConfig>().AsEnumerable();
            return dfcList;
        }

        public int GetLatestDatasetFileIdForDatasetByDatasetFileConfig(int datasetId, int dataFileConfigId, bool isBundled, string targetFileName = null, SchemaRevision schemaRevision = null)
        {
            int dfId = Query<DatasetFile>()
                .Where(w => w.Dataset.DatasetId == datasetId  && w.DatasetFileConfig.ConfigId == dataFileConfigId && w.ParentDatasetFileId == null && w.IsBundled == isBundled)
                .Where(w => ((targetFileName != null && w.FileName == targetFileName) || (targetFileName == null)) && ((schemaRevision != null && w.Schema == schemaRevision.ParentSchema) || (schemaRevision == null)))
                .GroupBy(x => x.Dataset.DatasetId)
                .ToList()
                .Select(s => s.OrderByDescending(g => g.CreateDTM).Take(1))
                .Select(i => i.First().DatasetFileId)
                .FirstOrDefault();

            return dfId;
        }

        public DatasetFileConfig getDatasetFileConfigs(int configId)
        {
            return Query<DatasetFileConfig>().Where(w => w.ConfigId == configId).FirstOrDefault();
        }

        public Interval GetInterval(string description)
        {
            return Query<Interval>().FirstOrDefault(x => x.Description.ToLower().Contains(description.ToLower()));
        }

        public List<Interval> GetAllIntervals()
        {
            return Query<Interval>().ToList();
        }

        public Interval GetInterval(int id)
        {
            return Query<Interval>().FirstOrDefault(x => x.Interval_ID == id);
        }

        public Status GetStatus(string description)
        {
            return Query<Status>().Cacheable().FirstOrDefault(x => x.Description.ToLower().Contains(description.ToLower()));
        }

        public Status GetStatus(int id)
        {
            return Query<Status>().Cacheable().FirstOrDefault(x => x.Status_ID == id);
        }

        public List<Status> GetAllStatuses()
        {
            return Query<Status>().Cacheable().ToList();
        }

        public List<Event> GetEvents(string reason)
        {
            return Query<Event>().Cacheable().Where(x => x.Reason.ToLower().Contains(reason.ToLower())).ToList();
        }

        public Event GetEvent(int id)
        {
            return Query<Event>().Cacheable().FirstOrDefault(x => x.EventID == id);
        }

        public List<Event> GetEventsStartedByUser(string SentryOwnerName)
        {
            return Query<Event>().Cacheable().Where(x => x.UserWhoStartedEvent == SentryOwnerName).ToList();
        }

        public bool IsUserSubscribedToDataset(string SentryOwnerName, int datasetID)
        {
            return Query<DatasetSubscription>().Cacheable().Any(x => x.SentryOwnerName == SentryOwnerName && x.Dataset.DatasetId == datasetID);
        }

        public bool IsUserSubscribedToDataAsset(string SentryOwnerName, int dataAssetID)
        {
            return Query<DataAssetSubscription>().Cacheable().Any(x => x.SentryOwnerName == SentryOwnerName && x.DataAsset.Id == dataAssetID);
        }

        public List<DatasetSubscription> GetAllUserSubscriptionsForDataset(string SentryOwnerName, int datasetID)
        {
            return Query<DatasetSubscription>().Where(x => x.SentryOwnerName == SentryOwnerName && x.Dataset.DatasetId == datasetID).ToList();
        }

        public List<BusinessAreaSubscription> GetAllUserSubscriptionsByEventTypeGroup(string SentryOwnerName,EventTypeGroup group)
        {

            if (group == EventTypeGroup.BusinessArea)
                return Query<BusinessAreaSubscription>().Where(x => x.SentryOwnerName == SentryOwnerName).ToList();
            else
                return null;
        }

        public List<DataAssetSubscription> GetAllUserSubscriptionsForDataAsset(string SentryOwnerName, int dataAssetID)
        {
            return Query<DataAssetSubscription>().Where(x => x.SentryOwnerName == SentryOwnerName && x.DataAsset.Id == dataAssetID).ToList();
        }

        public List<DatasetSubscription> GetAllSubscriptions()
        {
            return Query<DatasetSubscription>().ToList();
        }


        public List<DatasetSubscription> GetSubscriptionsForDataset(int datasetID)
        {
            return Query<DatasetSubscription>().Where(x => x.Dataset.DatasetId == datasetID).ToList();
        }

        public List<DataAssetSubscription> GetSubscriptionsForDataAsset(int dataAssetID)
        {
            return Query<DataAssetSubscription>().Cacheable().Where(x => x.DataAsset.Id == dataAssetID).ToList();
        }

        public List<Event> EventsSince(DateTime time, Boolean IsProcessed)
        {
            List<Event> events = Query<Event>().Where(e => e.TimeCreated >= time && e.IsProcessed == IsProcessed && (e.EventType.Display)).ToList();
            return events;
        }

        public int GetReportCount()
        {
            return Query<Dataset>().Where(x => x.DatasetType == GlobalConstants.DataEntityCodes.REPORT).Cacheable().Count();
        }

        public Favorite GetFavorite(int favoriteId)
        {
            return Query<Favorite>().Single(x => x.FavoriteId == favoriteId);
        }

        public List<Favorite> GetFavorites(List<int> favoriteIds)
        {
            return Query<Favorite>().Where(x => favoriteIds.Contains(x.FavoriteId)).ToList();
        }

        /// <summary>
        /// Generates unique value for storage location
        /// </summary>
        /// <returns></returns>
        public int GetNextStorageCDE()
        {
            string sqlConnString = null;
            string sqlQueryString = null;

            sqlConnString = Sentry.Configuration.Config.GetHostSetting("DatabaseConnectionString");
            sqlQueryString = $"SELECT NEXT VALUE FOR seq_StorageCDE";
            int result = ExecuteQuery(sqlConnString, sqlQueryString);

            //Reversing the order of the number for additional randomness in storage pattern.
            char[] charArray = result.ToString().ToCharArray();
            Array.Reverse(charArray);

            return Int32.Parse(new string(charArray));
        }

        private int ExecuteQuery(string sqlConnStr, string sqlQueryStr)
        {
            SqlConnection sqlConn = null;
            try
            {
                sqlConn = new SqlConnection(sqlConnStr);
                sqlConn.Open();
                SqlCommand sqlCmd = new SqlCommand(sqlQueryStr, sqlConn);
                SqlDataReader reader = sqlCmd.ExecuteReader();

                int result = 0;
                while (reader.Read())
                {
                    result = Int32.Parse(reader[0].ToString());                    
                }
                return result;
            }
            catch (Exception ex)
            {
                Sentry.Common.Logging.Logger.Error("Unable to execute query (" + sqlQueryStr + ") against (" + sqlConnStr + ")", ex);
                throw;
            }
            finally
            {
                if (sqlConn != null)
                {   // close connection
                    sqlConn.Close();
                    sqlConn.Dispose();
                }
            }
        }
    }
}
