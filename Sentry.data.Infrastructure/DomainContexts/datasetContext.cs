using System;
using System.Linq;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Linq;
using Sentry.NHibernate;
using Sentry.data.Core;
using System.Threading.Tasks;
using Sentry.data.Core.Entities.Metadata;
using System.Reflection;
using System.Collections;
using System.Web;
using System.Linq.Expressions;
using Sentry.data.Core.Entities;
using System.Data.SqlClient;

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

    public class datasetContext : NHWritableDomainContext, IDatasetContext
    {
        public datasetContext(ISession session) : base(session)
        {
            NHQueryableExtensionProvider.RegisterQueryableExtensionsProvider<datasetContext>();
        }


        public IQueryable<EventType> EventTypes
        {
            get
            {
                //TODO: Revisit for solution to filter based on user (i.e. Admins can see all eventtypes)
                return Query<EventType>().Cacheable();
            }
        }

        public IQueryable<DataSourceType> DataSourceTypes
        {
            get
            {
                //TODO: Revisit for solution to filter based on user (i.e. Admins can see all eventtypes)
                return Query<DataSourceType>().Cacheable();
            }
        }

        public IQueryable<DataSource> DataSources
        {
            get
            {
                //TODO: Revisit for solution to filter based on user (i.e. Admins can see all eventtypes)
                return Query<DataSource>().Cacheable();
            }
        }

        public IQueryable<AuthenticationType> AuthTypes
        {
            get
            {
                //TODO: Revisit for solution to filter based on user (i.e. Admins can see all eventtypes)
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
                return Query<Dataset>().Where(x => x.CanDisplay).Cacheable();
            }
        }

        public IQueryable<DataElement> DataElements
        {
            get
            {
                return Query<DataElement>().Cacheable();
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

        public IQueryable<Schema> Schemas
        {
            get
            {
                return Query<Schema>();  //QueryCacheRegion.MediumTerm
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

        public IEnumerable<Dataset> GetDatasetByCategoryID(int id)
        {
            return Query<Dataset>().Where(w => w.DatasetCategory.Id == id).Where(x => x.CanDisplay).AsEnumerable();
        }

        public Category GetCategoryById(int id)
        {
            return Query<Category>().Where(w => w.Id == id).FirstOrDefault();
        }


        public int GetDatasetCount()
        {

            return Query<Dataset>().Count(x => x.CanDisplay && x.DatasetType == null);
        }

        public IEnumerable<Dataset> GetExhibits()
        {
            return Query<Dataset>().Where(x => x.DatasetType == "RPT").Cacheable().AsEnumerable();
        }

        public Dataset GetById(int id)
        {
            Dataset ds = Query<Dataset>().Where((x) => x.DatasetId == id && x.CanDisplay).FirstOrDefault();
            return ds;
        }

        public Boolean isDatasetNameDuplicate(string datasetName, string category)
        {
            if (Query<Dataset>().Where(x => x.DatasetName == datasetName && x.Category == category).Count() == 0)
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

        /// <summary>
        /// Returns all versions of a datasetfile, including current.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IEnumerable<DatasetFile> GetDatasetFilesVersions(int datasetId, int dataFileConfigId, string filename)
        {
            IEnumerable<DatasetFile> list = Query<DatasetFile>().Where(x => x.Dataset.DatasetId == datasetId && x.DatasetFileConfig.ConfigId == dataFileConfigId && x.FileName == filename).Fetch(x => x.DatasetFileConfig).AsEnumerable();

            return list;
        }

        public IEnumerable<DatasetFile> GetAllDatasetFiles()
        {
            IEnumerable<DatasetFile> list = Query<DatasetFile>().Where(x => x.ParentDatasetFileId == null).Fetch(x => x.DatasetFileConfig).AsEnumerable();

            return list;
        }

        public DatasetFile GetDatasetFile(int id)
        {
            DatasetFile df = Query<DatasetFile>().Where(x => x.DatasetFileId == id).Fetch(x=> x.DatasetFileConfig).FirstOrDefault();
            return df;
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

        public int GetLatestDatasetFileIdForDatasetByDatasetFileConfig(int datasetId, int dataFileConfigId, bool isBundled, string targetFileName = null, DataElement schema = null)
        {
            int dfId = Query<DatasetFile>()
                .Where(w => w.Dataset.DatasetId == datasetId  && w.DatasetFileConfig.ConfigId == dataFileConfigId && w.ParentDatasetFileId == null && w.IsBundled == isBundled)
                .Where(w => ((targetFileName != null && w.FileName == targetFileName) || (targetFileName == null)) && ((schema != null && w.Schema == schema) || (schema == null)))
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
            return Query<Interval>().Cacheable().FirstOrDefault(x => x.Description.ToLower().Contains(description.ToLower()));
        }

        public List<Interval> GetAllIntervals()
        {
            return Query<Interval>().Cacheable().ToList();
        }

        public Interval GetInterval(int id)
        {
            return Query<Interval>().Cacheable().FirstOrDefault(x => x.Interval_ID == id);
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
            return Query<DatasetSubscription>().Cacheable().Where(x => x.SentryOwnerName == SentryOwnerName && x.Dataset.DatasetId == datasetID).ToList();
        }

        public List<DataAssetSubscription> GetAllUserSubscriptionsForDataAsset(string SentryOwnerName, int dataAssetID)
        {
            return Query<DataAssetSubscription>().Cacheable().Where(x => x.SentryOwnerName == SentryOwnerName && x.DataAsset.Id == dataAssetID).ToList();
        }

        public List<DatasetSubscription> GetAllSubscriptions()
        {
            return Query<DatasetSubscription>().Cacheable().ToList();
        }


        public List<DatasetSubscription> GetSubscriptionsForDataset(int datasetID)
        {
            return Query<DatasetSubscription>().Cacheable().Where(x => x.Dataset.DatasetId == datasetID).ToList();
        }

        public List<DataAssetSubscription> GetSubscriptionsForDataAsset(int dataAssetID)
        {
            return Query<DataAssetSubscription>().Cacheable().Where(x => x.DataAsset.Id == dataAssetID).ToList();
        }

        public List<Event> EventsSince(DateTime time, Boolean IsProcessed)
        {
            return Query<Event>().Cacheable().Where(e => e.TimeCreated >= time && e.IsProcessed == IsProcessed && e.EventType.Display).ToList();
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
            List<DataAssetHealth> healthList = new List<DataAssetHealth>();
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
                // TODO: log the exception... for now it's being eaten...
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
