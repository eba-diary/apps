using System;
using System.Linq;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Linq;
//using Sentry.Core;
using Sentry.NHibernate;
using Sentry.data.Core;
using System.Threading.Tasks;
using Sentry.data.Core.Entities.Metadata;
using System.Reflection;
using System.Collections;
using System.Web;

namespace Sentry.data.Infrastructure
{
    public class datasetContext : NHWritableDomainContext, IDatasetContext
    {
        public datasetContext(ISession session) : base(session)
        {
            NHQueryableExtensionProvider.RegisterQueryableExtensionsProvider<datasetContext>();
        }

        //public IQueryable<DomainUser> Users
        //{
        //    get
        //    {
        //        return Query<DomainUser>();
        //    }
        //}

        //###  BEGIN Sentry.Data  A### - Code below is Sentry.Data-specific
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

        public IQueryable<DatasetScopeType> DatasetScopeTypes
        {
            get
            {
                return Query<DatasetScopeType>().Cacheable();
            }
        }

        public class LineageCreation : IEnumerable<LineageCreation>
        {
            public virtual int ID { get; set; }
            public virtual int DataAsset_ID { get; set; }
            public virtual String DataElement_NME { get; set; }
            public virtual String DataObject_NME { get; set; }
            public virtual String DataObjectCode_DSC { get; set; }
            public virtual String DataObjectDetailType_VAL { get; set; }
            public virtual String DataObjectFieldDetailType_CDE { get; set; }
            public virtual String DataObjectFieldDetailType_VAL { get; set; }
            public virtual String DataObjectField_NME { get; set; }
            public virtual String DataObjectField_DSC { get; set; }
            public virtual String DataObject_DSC { get; set; }


            public IEnumerator<LineageCreation> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        public List<String> BusinessTerms(string dataElementCode, int? DataAsset_ID)
        {
            var rawQuery = Query<DataElement>()
                .Where(a => a.DataElement_CDE == dataElementCode)
                .Where(x => x.MetadataAsset.DataAsset_ID == DataAsset_ID);

            return rawQuery.Select(x => x.DataElement_NME).ToList();
        }




        public Lineage Description(int DataAsset_ID, string DataObject_NME, string DataObjectField_NME)
        {
            DataObject_NME = HttpUtility.UrlDecode(DataObject_NME);
            DataObjectField_NME = HttpUtility.UrlDecode(DataObjectField_NME);

            Lineage l = new Lineage();

            try
            {
                var dofDescription = Query<DataObjectField>()
                     .Where(a => a.DataObject.DataElement.DataElement_NME == "SERA PL.dm1")
                     .Where(b => DataObject_NME == b.DataObject.DataObject_NME)
                     .Where(c => DataObjectField_NME == c.DataObjectField_NME);

                l.DataObjectField_DSC = dofDescription.FirstOrDefault().DataObjectField_DSC;
            }
            catch (Exception ex)
            {
                //There was no description.
                //Return a null Lineage Object
            }

            try
            {
                var doDescription = Query<DataObject>()
                     .Where(a => a.DataElement.DataElement_NME == "SERA PL.dm1")
                     .Where(b => DataObject_NME == b.DataObject_NME);

                l.DataObject_DSC = doDescription.FirstOrDefault().DataObject_DSC;
            }
            catch (Exception ex)
            {
                //There was no description.
                //Return a null Lineage Object
            }



            return l;
        }

        public List<Lineage> Lineage(string dataElementCode, List<string> dataObjectFieldDetailTypes, int? DataAsset_ID, String DataElement_NME = "", String DataObject_NME = "", String DataObjectField_NME = "")
        {
            DataElement_NME = HttpUtility.UrlDecode(DataElement_NME);
            DataObject_NME = HttpUtility.UrlDecode(DataObject_NME);
            DataObjectField_NME = HttpUtility.UrlDecode(DataObjectField_NME);

            var rawQuery = Query<DataObjectFieldDetail>()
                .Where(a => a.DataObjectField.DataObject.DataElement.DataElement_CDE == dataElementCode)
                .Where(b => dataObjectFieldDetailTypes.Contains(b.DataObjectFieldDetailType_CDE));

            if (DataAsset_ID != null)
            {
                rawQuery = rawQuery.Where(c => c.DataObjectField.DataObject.DataElement.MetadataAsset.DataAsset_ID == DataAsset_ID);
            }
            if (!string.IsNullOrEmpty(DataElement_NME))
            {
                rawQuery = rawQuery.Where(c => c.DataObjectField.DataObject.DataElement.DataElement_NME == DataElement_NME);
            }
            if (!string.IsNullOrEmpty(DataObject_NME))
            {
                rawQuery = rawQuery.Where(d => d.DataObjectField.DataObject.DataObject_NME == DataObject_NME);
            }
            if (!string.IsNullOrEmpty(DataObjectField_NME))
            {
                rawQuery = rawQuery.Where(e => e.DataObjectField.DataObjectField_NME == DataObjectField_NME);
            }

            var query = rawQuery.Select(x => new LineageCreation
            {
                DataAsset_ID = x.DataObjectField.DataObject.DataElement.MetadataAsset.DataAsset_ID,
                DataElement_NME = x.DataObjectField.DataObject.DataElement.DataElement_NME,
                DataObject_NME = x.DataObjectField.DataObject.DataObject_NME,
               // DataObject_DSC = x.DataObjectField.DataObject.DataObject_DSC,
                DataObjectCode_DSC = x.DataObjectField.DataObject.DataObjectCode_DSC,
                DataObjectField_NME = x.DataObjectField.DataObjectField_NME,
               // DataObjectField_DSC = x.DataObjectField.DataObjectField_DSC,
                DataObjectFieldDetailType_CDE = x.DataObjectFieldDetailType_CDE,
                DataObjectFieldDetailType_VAL = x.DataObjectFieldDetailType_VAL
            });



            List<Lineage> lineage = new List<Lineage>();

            try
            {
                foreach (var item in query)
                {

                    var found = lineage.FirstOrDefault(x => x.DataAsset_ID == item.DataAsset_ID &&
                        x.DataElement_NME == item.DataElement_NME &&
                        x.DataObject_NME == item.DataObject_NME &&
                        x.DataObjectCode_DSC == item.DataObjectCode_DSC &&
                        x.DataObjectDetailType_VAL == item.DataObjectDetailType_VAL &&
                        x.DataObjectField_NME == item.DataObjectField_NME);

                    Lineage l;

                    if (found == null)
                    {
                        l = new Lineage();

                        l.DataAsset_ID = item.DataAsset_ID;
                        l.DataElement_NME = item.DataElement_NME;
                        l.DataObject_NME = item.DataObject_NME;
                        l.DataObjectCode_DSC = item.DataObjectCode_DSC;
                        l.DataObjectDetailType_VAL = item.DataObjectDetailType_VAL;
                        l.DataObjectField_NME = item.DataObjectField_NME;
                        l.ID = lineage.Count;
                    }
                    else
                    {
                        l = found;
                    }

                    switch (item.DataObjectFieldDetailType_CDE)
                    {
                        case "SourceElement_NME":
                            l.SourceElement_NME = item.DataObjectFieldDetailType_VAL;
                            break;
                        case "SourceObject_NME":
                            l.SourceObject_NME = item.DataObjectFieldDetailType_VAL;
                            break;
                        case "SourceObjectField_NME":
                            l.SourceObjectField_NME = item.DataObjectFieldDetailType_VAL;
                            break;
                        case "Source_TXT":
                            l.Source_TXT = item.DataObjectFieldDetailType_VAL;
                            break;
                        case "Transformation_TXT":
                            l.Transformation_TXT = item.DataObjectFieldDetailType_VAL;
                            break;
                        case "Display_IND":
                            //l.Display_IND = Convert.ToInt32(item.DataObjectFieldDetailType_VAL);
                            break;
                        case "MultipleSourceField_IND":
                            //l.MultipleSourceField_IND = Convert.ToInt32(item.DataObjectFieldDetailType_VAL);
                            break;
                    }

                    if (found == null)
                    {
                        lineage.Add(l);
                    }

                }
            }
            catch(Exception ex)
            {
                var a = 0;
            }

            return lineage;
        }
        

        public IQueryable<Category> Categories
        {
            get
            {
                return Query<Category>().Cacheable();  //QueryCacheRegion.MediumTerm
            }

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
                return Query<AuthenticationType>().Cacheable();
            }
        }



        public IQueryable<Status> EventStatus
        {
            get
            {
                return Query<Status>().Cacheable();
            }
        }

        //public IQueryable<DatasetMetadata> DatasetMetadata
        //{
        //    get
        //    {
        //       // return Query<DatasetMetadata>().Cacheable(); //QueryCacheRegion.MediumTerm
        //    }
        //}

        public int GetDatasetCount()
        {
            return Query<Dataset>().Where(x => x.CanDisplay).Cacheable().Count();
        }

        public int GetCategoryDatasetCount(Category cat)
        {
            return Query<Dataset>().Cacheable().Where(w => w.Category == cat.Name && w.CanDisplay).Count();
        }

        public int GetMaxId()
        {
            int maxId = Query<Dataset>().Max((x) => x.DatasetId);
            return maxId;
        }

        public Dataset GetById(int id)
        {
            Dataset ds = Query<Dataset>().Where((x) => x.DatasetId == id && x.CanDisplay).FirstOrDefault();
            return ds;
        }

        public Boolean s3KeyDuplicate(string s3key)
        {
            if (Query<Dataset>().Where(x => x.S3Key == s3key).Count() == 0){
                return false;
            }
            else
            {
                return true;
            }
        }

        public Boolean s3KeyDuplicate(int datasetId, string s3key)
        {
            if (Query<DatasetFile>().Where(x => x.Dataset.DatasetId == datasetId && x.FileLocation == s3key).Count() == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public Category GetCategoryByName(string name)
        {
            return Query<Category>().Where(x => x.Name == name).FirstOrDefault();
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

        public IEnumerable<String> GetCategoryList()
        {
            return Query<Dataset>().Select(s => s.Category).Distinct().AsEnumerable();
            //return catList;
        }

        public IEnumerable<String> GetSentryOwnerList()
        {
            IEnumerable<string> list = Query<Dataset>().Where(x => x.CanDisplay).Select((x) => x.SentryOwnerName).Distinct().AsEnumerable();
            return list;
        }

        public void DeleteAllData()
        {
            DemoDataService.DeleteAllDemoData(this.Session);
        }

        public IEnumerable<DatasetFrequency> GetDatasetFrequencies()
        {
            IEnumerable<DatasetFrequency> values = Enum.GetValues(typeof(DatasetFrequency)).Cast<DatasetFrequency>();
            return values;
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
        public IEnumerable<DatasetFile> GetDatasetFilesForDataset(int id, Func<DatasetFile, bool> where)
        {
            IEnumerable<DatasetFile> list = 
                Query<DatasetFile>().Where
                (
                    x => x.Dataset.DatasetId == id && 
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

        public IEnumerable<Dataset> GetDatasetByCategoryID(int id)
        {
            return Query<Dataset>().Where(w => w.DatasetCategory.Id == id).Where(x => x.CanDisplay).AsEnumerable();
        }

        public Category GetCategoryById(int id)
        {
            return Query<Category>().Where(w => w.Id == id).FirstOrDefault();
        }

        public IEnumerable<DatasetScopeType> GetAllDatasetScopeTypes()
        {
            return Query<DatasetScopeType>().AsEnumerable();
        }

        public DatasetScopeType GetDatasetScopeById(int id)
        {
            return Query<DatasetScopeType>().Where(w => w.ScopeTypeId == id).FirstOrDefault();
        }

        public Dataset GetByS3Key(string s3Key)
        {
            return Query<Dataset>().Where(s => s.S3Key == s3Key).FirstOrDefault();
        }

        public IEnumerable<DatasetFileConfig> getAllDatasetFileConfigs()
        {
            IEnumerable<DatasetFileConfig> dfcList = Query<DatasetFileConfig>().AsEnumerable();
            return dfcList;
        }

        public int GetLatestDatasetFileIdForDatasetByDatasetFileConfig(int datasetId, int dataFileConfigId, bool isBunled)
        {
            int dfId = Query<DatasetFile>().Where(w => w.Dataset.DatasetId == datasetId && w.DatasetFileConfig.ConfigId == dataFileConfigId && w.ParentDatasetFileId == null && w.IsBundled == isBunled).GroupBy(x => x.Dataset.DatasetId).ToList().Select(s => s.OrderByDescending(g => g.CreateDTM).Take(1)).Select(i => i.First().DatasetFileId).FirstOrDefault();
            return dfId;
        }

        public int GetLatestBundleFileIdForDatasetByDatasetFileConfig(int datasetId, int dataFileConfigId)
        {
            int dfId = Query<DatasetFile>().Where(w => w.Dataset.DatasetId == datasetId && w.DatasetFileConfig.ConfigId == dataFileConfigId && w.ParentDatasetFileId == null && w.IsBundled).GroupBy(x => x.Dataset.DatasetId).ToList().Select(s => s.OrderByDescending(g => g.CreateDTM).Take(1)).Select(i => i.First().DatasetFileId).FirstOrDefault();

            return dfId;
        }

        public int GetLatestDatasetFileIdForDatasetByDatasetFileConfig(int datasetId, int dataFileConfigId, string targetFileName, bool isBundled)
        {
            int dfId = Query<DatasetFile>().Where(w => w.Dataset.DatasetId == datasetId && w.ParentDatasetFileId == null && w.DatasetFileConfig.ConfigId == dataFileConfigId && w.FileName == targetFileName && w.IsBundled == isBundled).GroupBy(x => x.Dataset.DatasetId).ToList().Select(s => s.OrderByDescending(g => g.CreateDTM).Take(1)).Select(i => i.First().DatasetFileId).FirstOrDefault();

            return dfId;
        }

        public DatasetFileConfig getDatasetFileConfigs(int configId)
        {
            return Query<DatasetFileConfig>().Where(w => w.ConfigId == configId).FirstOrDefault();
        }
        public DatasetFileConfig getDatasetDefaultConfig(int datasetId)
        {
            List<DatasetFileConfig> dfcList = Query<DatasetFileConfig>().Where(w => w.ParentDataset.DatasetId == datasetId && w.IsGeneric).ToList();
            return dfcList.FirstOrDefault();

            //return Query<DatasetFileConfig>().Where(w => w.IsGeneric).FirstOrDefault();
        }

        public IEnumerable<AssetNotifications> GetAssetNotificationsByDataAssetId(int id)
        {
            return Query<AssetNotifications>().Where(w => w.ParentDataAsset.Id == id).ToList();
        }
        public IEnumerable<AssetNotifications> GetAllAssetNotifications()
        {
            return Query<AssetNotifications>().ToList();
        }
        public AssetNotifications GetAssetNotificationByID(int id)
        {
            return Query<AssetNotifications>().Where(w => w.NotificationId == id).First();
        }
        public IList<DataAsset> GetDataAssets()
        {
            return Query<DataAsset>().Cacheable().OrderBy(x => x.Name).ToList();
        }
        public DataAsset GetDataAsset(int id)
        {
            //DataAsset da = Query<DataAsset>().Cacheable().Where(x => x.Id == id).FetchMany(x => x.Components).ToList().FirstOrDefault();
            DataAsset da = Query<DataAsset>().Cacheable().Where(x => x.Id == id).ToList().FirstOrDefault();
            return da;
        }

        public DataAsset GetDataAsset(string assetName)
        {
            //DataAsset da = Query<DataAsset>().Cacheable().Where(x => x.Name == assetName).FetchMany(x => x.Components).ToList().FirstOrDefault();
            DataAsset da = Query<DataAsset>().Cacheable().Where(x => x.Name == assetName).ToList().FirstOrDefault();

            return da;
        }



        public Interval GetInterval(string description)
        {
            return Query<Interval>().Cacheable().Where(x => x.Description.ToLower().Contains(description.ToLower())).FirstOrDefault();
        }

        public List<Interval> GetAllIntervals()
        {
            return Query<Interval>().Cacheable().ToList();
        }

        public Interval GetInterval(int id)
        {
            return Query<Interval>().Cacheable().Where(x => x.Interval_ID == id).FirstOrDefault();
        }

        public Status GetStatus(string description)
        {
            return Query<Status>().Cacheable().Where(x => x.Description.ToLower().Contains(description.ToLower())).FirstOrDefault();
        }

        public Status GetStatus(int id)
        {
            return Query<Status>().Cacheable().Where(x => x.Status_ID == id).FirstOrDefault();
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
            return Query<Event>().Cacheable().Where(x => x.EventID == id).FirstOrDefault();
        }

        public List<Event> GetEventsStartedByUser(string SentryOwnerName)
        {
            return Query<Event>().Cacheable().Where(x => x.UserWhoStartedEvent == SentryOwnerName).ToList();
        }

        public bool IsUserSubscribedToDataset(string SentryOwnerName, int datasetID)
        {
            return Query<DatasetSubscription>().Cacheable().Where(x => x.SentryOwnerName == SentryOwnerName && x.Dataset.DatasetId == datasetID).Any();
        }

        public bool IsUserSubscribedToDataAsset(string SentryOwnerName, int dataAssetID)
        {
            return Query<DataAssetSubscription>().Cacheable().Where(x => x.SentryOwnerName == SentryOwnerName && x.DataAsset.Id == dataAssetID).Any();
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
            return Query<Event>().Cacheable().Where(e => e.TimeCreated >= time && e.IsProcessed == IsProcessed).ToList();
        }


        //public Task MergeAsync<T> ()
        //{
        //    return Task.Factory.StartNew(() => {
                
        //        session.Save(user);
        //    }).ContinueWith(ex => Trace.TraceError(ex?.Exception?.Message ?? "Strange task fault"), TaskContinuationOptions.OnlyOnFaulted);
        //}




    }
}
