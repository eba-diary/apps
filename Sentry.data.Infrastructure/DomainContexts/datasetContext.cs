using System;
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
                IQueryable<Dataset> qResult = Query<Dataset>().Cacheable(QueryCacheRegion.MediumTerm);
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

        //public IQueryable<DatasetMetadata> DatasetMetadata
        //{
        //    get
        //    {
        //        return Query<DatasetMetadata>().Cacheable(QueryCacheRegion.MediumTerm);
        //    }
        //}

        public int GetMaxId()
        {
            int maxId = Query<Dataset>().Max((x) => x.DatasetId);
            return maxId;
        }

        public Dataset GetById(int id)
        {
            Dataset ds = Query<Dataset>().Where((x) => x.DatasetId == id).FirstOrDefault();
            return ds;
        }

        public IEnumerable<String> GetCategoryList()
        {
            return Query<Dataset>().Select(s => s.Category).Distinct().AsEnumerable();
            //return catList;
        }

        public void DeleteAllData()
        {
            DemoDataService.DeleteAllDemoData(this.Session);
        }
        //###  END Sentry.Data  ### - Code above is Sentry.Data-specific
    }
}
