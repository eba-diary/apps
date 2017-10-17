﻿using System;
using System.Linq;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Linq;
using Sentry.Core;
using Sentry.NHibernate;
using Sentry.data.Core;

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
                IQueryable<Dataset> qResult = Query<Dataset>().Where(x => x.CanDisplay).Cacheable(QueryCacheRegion.MediumTerm);
                //if (qResult != null && qResult.Count() > 0)
                //{
                //    foreach (Dataset qRow in qResult)
                //    {
                //        if (qRow.Columns != null && qRow.Columns.Count == 0)
                //        {
                //            qRow.Columns = null;
                //        }
                //        if (qRow.Metadata != null && qRow.Metadata.Count == 0)
                //        {
                //            qRow.Metadata = null;
                //        }
                //    }
                //}
                return qResult;
            }
        }

        public IQueryable<Category> Categories
        {
            get
            {
                return Query<Category>().Cacheable(QueryCacheRegion.MediumTerm);
            }

        }

        public IQueryable<DatasetMetadata> DatasetMetadata
        {
            get
            {
                return Query<DatasetMetadata>().Cacheable(QueryCacheRegion.MediumTerm);
            }
        }

        public int GetDatasetCount()
        {
            return Query<Dataset>().Where(x => x.CanDisplay).Cacheable().Count();
        }

        public int GetCategoryDatasetCount(Category cat)
        {
            return Query<Dataset>().Cacheable().Where(w => w.DatasetCategory.Id == cat.Id && w.CanDisplay).Count();
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
        public IEnumerable<DatasetFile> GetDatasetFilesForDataset(int id)
        {
            IEnumerable<DatasetFile> list = Query<DatasetFile>().Where(x => x.Dataset.DatasetId == id && x.ParentDatasetFileId == null).AsEnumerable();

            return list;
        }

        /// <summary>
        /// Returns all versions of a datasetfile, including current.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IEnumerable<DatasetFile> GetDatasetFilesVersions(int datasetId, int dataFileConfigId, string filename)
        {
            IEnumerable<DatasetFile> list = Query<DatasetFile>().Where(x => x.Dataset.DatasetId == datasetId && x.DatasetFileConfig.DataFileConfigId == dataFileConfigId && x.FileName == filename).AsEnumerable();

            return list;
        }

        public IEnumerable<DatasetFile> GetAllDatasetFiles()
        {
            IEnumerable<DatasetFile> list = Query<DatasetFile>().Where(x => x.ParentDatasetFileId == null).AsEnumerable();

            return list;
        }

        public DatasetFile GetDatasetFile(int id)
        {
            DatasetFile df = Query<DatasetFile>().Where(x => x.DatasetFileId == id).FirstOrDefault();
            return df;
        }

        public int GetLatestDatasetFileIdForDataset(int id)
        {
            int d = Query<DatasetFile>().Where(w => w.Dataset.DatasetId == id).GroupBy(x => x.Dataset.DatasetId).ToList().Select(s => s.OrderByDescending(g => g.CreateDTM).Take(1)).Select(i => i.First().DatasetFileId).FirstOrDefault();
            //DatasetFile df = Query<DatasetFile>().GroupBy(x => x.DatasetId).SelectMany(s => s.OrderByDescending(g => g.CreateDTM).Take(1)).FirstOrDefault();
            //Query<DatasetFile>().Where(d => d.DatasetId == id).GroupBy(x => x.DatasetId).Select(s => s.Select(i => i.CreateDTM).Max());
            ////DatasetFile df = Query<DatasetFile>().Where(x => x.DatasetId == id && x.DatasetFileId == Query<DatasetFile>().Max(m => m.DatasetFileId)).fir;
            //return df.DatasetFileId;
            return d;
        }

        public IEnumerable<Dataset> GetDatasetByCategoryID(int id)
        {
            return Query<Dataset>().Where(w => w.DatasetCategory.Id == id).AsEnumerable();
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

        public int GetLatestDatasetFileIdForDatasetByDatasetFileConfig(int datasetId, int dataFileConfigId)
        {
            int dfId = Query<DatasetFile>().Where(w => w.Dataset.DatasetId == datasetId && w.DatasetFileConfig.DataFileConfigId == dataFileConfigId && w.ParentDatasetFileId == null).GroupBy(x => x.Dataset.DatasetId).ToList().Select(s => s.OrderByDescending(g => g.CreateDTM).Take(1)).Select(i => i.First().DatasetFileId).FirstOrDefault();

            return dfId;
        }

        public int GetLatestDatasetFileIdForDatasetByDatasetFileConfig(int datasetId, int dataFileConfigId, string targetFileName)
        {
            int dfId = Query<DatasetFile>().Where(w => w.Dataset.DatasetId == datasetId && w.ParentDatasetFileId == null && w.DatasetFileConfig.DataFileConfigId == dataFileConfigId && w.FileName == targetFileName).GroupBy(x => x.Dataset.DatasetId).ToList().Select(s => s.OrderByDescending(g => g.CreateDTM).Take(1)).Select(i => i.First().DatasetFileId).FirstOrDefault();

            return dfId;
        }

        public DatasetFileConfig getDatasetFileConfigs(int configId)
        {
            return Query<DatasetFileConfig>().Where(w => w.ConfigId == configId).FirstOrDefault();
        }
        //###  END Sentry.Data  ### - Code above is Sentry.Data-specific
    }
}
