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

        public List<String> BusinessTerms(string dataElementCode, int? DataAsset_ID, String DataElement_NME = "", String DataObject_NME = "", String DataObjectField_NME = "", String Line_CDE = "")
        {
            DataElement_NME = HttpUtility.UrlDecode(DataElement_NME);
            DataObject_NME = HttpUtility.UrlDecode(DataObject_NME);
            DataObjectField_NME = HttpUtility.UrlDecode(DataObjectField_NME);
            Line_CDE = HttpUtility.UrlDecode(Line_CDE);

            var rawQuery = Query<Lineage>().Where(x => x.DataElement_TYP == DataElementCode.BusinessTerm);

            if (DataAsset_ID != null)
            {
                rawQuery = rawQuery.Where(c => c.DataAsset_ID == DataAsset_ID);
            }
            if (!string.IsNullOrEmpty(Line_CDE))
            {
                rawQuery = rawQuery.Where(c => c.Line_CDE == Line_CDE);
            }
            if (!string.IsNullOrEmpty(DataElement_NME))
            {
                rawQuery = rawQuery.Where(c => c.SourceElement_NME == DataElement_NME);
            }
            if (!string.IsNullOrEmpty(DataObject_NME))
            {
                rawQuery = rawQuery.Where(d => d.SourceObject_NME == DataObject_NME);
            }

            return rawQuery.Select(x => x.DataElement_NME).ToList();
        }

        public List<String> ConsumptionLayers(string dataElementCode, int? DataAsset_ID, String DataElement_NME = "", String DataObject_NME = "", String DataObjectField_NME = "", String Line_CDE = "")
        {
            DataElement_NME = HttpUtility.UrlDecode(DataElement_NME);
            DataObject_NME = HttpUtility.UrlDecode(DataObject_NME);
            DataObjectField_NME = HttpUtility.UrlDecode(DataObjectField_NME);
            Line_CDE = HttpUtility.UrlDecode(Line_CDE);

            var rawQuery = Query<Lineage>().Where(x => x.DataElement_TYP == DataElementCode.BusinessTerm);

            if (DataAsset_ID != null)
            {
                rawQuery = rawQuery.Where(c => c.DataAsset_ID == DataAsset_ID);
            }
            if (!string.IsNullOrEmpty(Line_CDE))
            {
                rawQuery = rawQuery.Where(c => c.Line_CDE == Line_CDE);
            }
            if (!string.IsNullOrEmpty(DataObject_NME))
            {
                rawQuery = rawQuery.Where(d => d.SourceObject_NME == DataObject_NME);
            }
            if (!string.IsNullOrEmpty(DataObjectField_NME))
            {
                var predicate = PredicateBuilder.False<Lineage>();

                var BusinessTermSources = Query<Lineage>()
                    .Where(x => x.DataElement_TYP == DataElementCode.BusinessTerm)
                    .Where(x => x.DataElement_NME == DataObjectField_NME);

                if (DataAsset_ID != null)
                {
                    BusinessTermSources = BusinessTermSources.Where(c => c.DataAsset_ID == DataAsset_ID);
                }
                if (!string.IsNullOrEmpty(Line_CDE))
                {
                    BusinessTermSources = BusinessTermSources.Where(c => c.Line_CDE == Line_CDE);
                }

                foreach (string keyword in BusinessTermSources.Select(x => x.SourceField_NME).ToList())
                {
                    string temp = keyword;
                    predicate = predicate.Or(p => p.SourceField_NME == temp);
                }

                rawQuery = rawQuery.Where(predicate);
            }

            return rawQuery.Select(x => x.SourceElement_NME).ToList();
        }


        public List<String> LineageTables(string dataElementCode, int? DataAsset_ID, String DataElement_NME = "", String DataObject_NME = "", String DataObjectField_NME = "", String Line_CDE = "")
        {
            DataElement_NME = HttpUtility.UrlDecode(DataElement_NME);
            DataObject_NME = HttpUtility.UrlDecode(DataObject_NME);
            DataObjectField_NME = HttpUtility.UrlDecode(DataObjectField_NME);
            Line_CDE = HttpUtility.UrlDecode(Line_CDE);

            var rawQuery = Query<Lineage>().Where(x => x.DataElement_TYP == DataElementCode.BusinessTerm);

            if (DataAsset_ID != null)
            {
                rawQuery = rawQuery.Where(c => c.DataAsset_ID == DataAsset_ID);
            }
            if (!string.IsNullOrEmpty(Line_CDE))
            {
                rawQuery = rawQuery.Where(c => c.Line_CDE == Line_CDE);
            }
            if (!string.IsNullOrEmpty(DataElement_NME))
            {
                rawQuery = rawQuery.Where(c => c.SourceElement_NME == DataElement_NME);
            }
            if (!string.IsNullOrEmpty(DataObjectField_NME))
            {
                var predicate = PredicateBuilder.False<Lineage>();

                var BusinessTermSources = Query<Lineage>()
                   .Where(x => x.DataElement_TYP == DataElementCode.BusinessTerm)
                   .Where(x => x.DataElement_NME == DataObjectField_NME);

                if (DataAsset_ID != null)
                {
                    BusinessTermSources = BusinessTermSources.Where(c => c.DataAsset_ID == DataAsset_ID);
                }
                if (!string.IsNullOrEmpty(Line_CDE))
                {
                    BusinessTermSources = BusinessTermSources.Where(c => c.Line_CDE == Line_CDE);
                }

                foreach (string keyword in BusinessTermSources.Select(x => x.SourceField_NME).ToList())
                {
                    string temp = keyword;
                    predicate = predicate.Or(p => p.SourceField_NME == temp);
                }

                rawQuery = rawQuery.Where(predicate);
            }

            return rawQuery.Select(x => x.SourceObject_NME).ToList();
        }

        public List<String> BusinessTermDescription(string dataElementCode, int? DataAsset_ID, string DataObjectField_NME, String Line_CDE = "")
        {
            DataObjectField_NME = HttpUtility.UrlDecode(DataObjectField_NME);
            Line_CDE = HttpUtility.UrlDecode(Line_CDE);

            var rawQuery = Query<Lineage>().Where(x => x.DataElement_TYP == dataElementCode);

            if (DataAsset_ID != null)
            {
                rawQuery = rawQuery.Where(c => c.DataAsset_ID == DataAsset_ID);
            }
            if (!string.IsNullOrEmpty(Line_CDE))
            {
                rawQuery = rawQuery.Where(c => c.Line_CDE == Line_CDE);
            }
            if (!string.IsNullOrEmpty(DataObjectField_NME))
            {
                var predicate = PredicateBuilder.False<Lineage>();

                var BusinessTermSources = Query<Lineage>()
                   .Where(x => x.DataElement_TYP == DataElementCode.BusinessTerm)
                   .Where(x => x.DataElement_NME == DataObjectField_NME);

                if (DataAsset_ID != null)
                {
                    BusinessTermSources = BusinessTermSources.Where(c => c.DataAsset_ID == DataAsset_ID);
                }
                if (!string.IsNullOrEmpty(Line_CDE))
                {
                    BusinessTermSources = BusinessTermSources.Where(c => c.Line_CDE == Line_CDE);
                }

                foreach (string keyword in BusinessTermSources.Select(x => x.SourceField_NME).ToList())
                {
                    string temp = keyword;
                    predicate = predicate.Or(p => p.SourceField_NME == temp);
                }

                rawQuery = rawQuery.Where(predicate);
            }

            return rawQuery.Select(x => x.BusTerm_DSC).ToList();
        }

        public Lineage Description(int? DataAsset_ID, string DataObject_NME, string DataObjectField_NME, String Line_CDE = "")
        {
            DataObject_NME = HttpUtility.UrlDecode(DataObject_NME);
            DataObjectField_NME = HttpUtility.UrlDecode(DataObjectField_NME);
            Line_CDE = HttpUtility.UrlDecode(Line_CDE);

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

        public List<LineageCreation> Lineage(string dataElementCode, int? DataAsset_ID, String DataElement_NME = "", String DataObject_NME = "", String DataObjectField_NME = "", String Line_CDE = "")
        {
            DataElement_NME = HttpUtility.UrlDecode(DataElement_NME);
            DataObject_NME = HttpUtility.UrlDecode(DataObject_NME);
            DataObjectField_NME = HttpUtility.UrlDecode(DataObjectField_NME);
            Line_CDE = HttpUtility.UrlDecode(Line_CDE);

            var rawQuery = Query<Lineage>();

            if (DataAsset_ID != null)
            {
                rawQuery = rawQuery.Where(c => c.DataAsset_ID == DataAsset_ID);
            }
            if (!string.IsNullOrEmpty(Line_CDE))
            {
                rawQuery = rawQuery.Where(c => c.Line_CDE == Line_CDE);
            }
            if (!string.IsNullOrEmpty(DataElement_NME))
            {
                rawQuery = rawQuery.Where(c => c.DataElement_NME == DataElement_NME);
            }
            if (!string.IsNullOrEmpty(DataObject_NME))
            {
                rawQuery = rawQuery.Where(d => d.DataObject_NME == DataObject_NME);
            }
            if (!string.IsNullOrEmpty(DataObjectField_NME))
            {
                var predicate = PredicateBuilder.False<Lineage>();

                var BusinessTermSources = Query<Lineage>()
                    .Where(x => x.DataElement_TYP == DataElementCode.BusinessTerm)
                    .Where(x => x.DataElement_NME == DataObjectField_NME);

                if (DataAsset_ID != null)
                {
                    BusinessTermSources = BusinessTermSources.Where(c => c.DataAsset_ID == DataAsset_ID);
                }
                if (!string.IsNullOrEmpty(Line_CDE))
                {
                    BusinessTermSources = BusinessTermSources.Where(c => c.Line_CDE == Line_CDE);
                }

                foreach (string keyword in BusinessTermSources.Select(x => x.SourceField_NME).ToList())
                {
                    string temp = keyword;
                    predicate = predicate.Or(p => p.DataObjectField_NME == temp);
                }

                rawQuery = rawQuery.Where(predicate);
            }

            List<LineageCreation> masterList = new List<LineageCreation>();

            List<Lineage> allLineage = Query<Lineage>().ToList();

            foreach (Lineage l in rawQuery)
            {
                LineageCreation lc = masterList.FirstOrDefault(a => a.DataElement_NME == l.DataElement_NME && a.DataObject_NME == l.DataObject_NME && a.DataObjectField_NME == l.DataObjectField_NME);

                if(lc != null)
                {
                    lc.SourceElement_NME = l.SourceElement_NME;
                    lc.SourceField_NME = l.SourceField_NME;
                    lc.SourceObject_NME = l.SourceObject_NME;
                }
                else
                {
                    lc = new LineageCreation()
                    {
                        DataAsset_ID = l.DataAsset_ID,
                        Layer = 0,

                        Model_NME = l.Model_NME,

                        DataElement_NME = l.DataElement_NME,
                        DataElement_TYP = l.DataElement_TYP,

                        DataObject_NME = l.DataObject_NME,
                        DataObject_DSC = l.DataObject_DSC,
                        DataObjectCode_DSC = l.DataObjectCode_DSC,

                        DataObjectDetailType_VAL = l.DataObjectDetailType_VAL,
                        DataObjectField_NME = l.DataObjectField_NME,
                        DataObjectField_DSC = l.DataObjectField_DSC,

                        SourceElement_NME = l.SourceElement_NME,
                        SourceField_NME = l.SourceField_NME,
                        SourceObject_NME = l.SourceObject_NME,

                        Display_IND = l.Display_IND,
                        Sources = new List<LineageCreation>(),
                        DataLineage_ID = l.DataLineage_ID,
                        Transformation_TXT = l.Transformation_TXT,
                        Source_TXT = l.Source_TXT
                    };

                    masterList.Add(lc);
                }

                foreach (var source in RecursiveSources(lc, allLineage))
                {
                    if (!lc.Sources.Any(x => x.DataLineage_ID == source.DataLineage_ID))
                    {
                        lc.Sources.Add(source);
                    }
                }
            }

            return masterList.ToList();
        }


        //private List<LineageCreation> RecursiveSources(LineageCreation parent, List<Lineage> allLineage, List<LineageCreation> cachedList)
        //{
        //    var children = allLineage.Where(a =>
        //        a.DataObject_NME.Trim().ToLower() == parent.SourceObject_NME.Trim().ToLower()
        //        && a.DataObjectField_NME.Trim().ToLower() == parent.SourceField_NME.Trim().ToLower()
        //        && a.DataElement_NME.Trim().ToLower() == parent.SourceElement_NME.Trim().ToLower()
        //    ).ToList();

        //    if (parent.Sources == null)
        //    {
        //        parent.Sources = new List<LineageCreation>();
        //    }

        //    foreach (Lineage child in children)
        //    {
        //        //Search the Cached List for the Child
        //        LineageCreation cachedChild = cachedList.FirstOrDefault(cache =>
        //            cache.DataElement_NME == child.DataElement_NME
        //                && cache.DataObject_NME == child.DataObject_NME
        //                && cache.DataObjectField_NME == child.DataObjectField_NME
        //                && cache.SourceElement_NME == child.SourceElement_NME
        //                && cache.SourceObject_NME == child.SourceObject_NME
        //                && cache.SourceField_NME == child.SourceField_NME
        //            );

        //        if (cachedChild != null)
        //        {
        //            parent.Sources.Add(cachedChild);
        //        }
        //        else
        //        {

        //            LineageCreation candidateChild = parent.Sources.FirstOrDefault(a =>
        //            a.DataElement_NME == child.DataElement_NME
        //            && a.DataObject_NME == child.DataObject_NME
        //            && a.DataObjectField_NME == child.DataObjectField_NME);

        //            if (candidateChild != null)
        //            {
        //                candidateChild.SourceElement_NME = child.SourceElement_NME;
        //                candidateChild.SourceField_NME = child.SourceField_NME;
        //                candidateChild.SourceObject_NME = child.SourceObject_NME;
        //            }
        //            else
        //            {
        //                candidateChild = new LineageCreation()
        //                {
        //                    DataAsset_ID = child.DataAsset_ID,
        //                    Layer = parent.Layer + 1,

        //                    Model_NME = child.Model_NME,

        //                    DataElement_NME = child.DataElement_NME,
        //                    DataElement_TYP = child.DataElement_TYP,

        //                    DataObject_NME = child.DataObject_NME,
        //                    DataObject_DSC = child.DataObject_DSC,
        //                    DataObjectCode_DSC = child.DataObjectCode_DSC,

        //                    DataObjectDetailType_VAL = child.DataObjectDetailType_VAL,
        //                    DataObjectField_NME = child.DataObjectField_NME,
        //                    DataObjectField_DSC = child.DataObjectField_DSC,

        //                    SourceElement_NME = child.SourceElement_NME,
        //                    SourceField_NME = child.SourceField_NME,
        //                    SourceObject_NME = child.SourceObject_NME,

        //                    Display_IND = child.Display_IND,
        //                    Sources = new List<LineageCreation>(),

        //                    DataLineage_ID = child.DataLineage_ID,
        //                    Transformation_TXT = child.Transformation_TXT,
        //                    Source_TXT = child.Source_TXT
        //                };

        //                parent.Sources.Add(candidateChild);
        //            }

        //            foreach (var source in RecursiveSources(candidateChild, allLineage, cachedList))
        //            {
        //                if (!candidateChild.Sources.Any(x => x.DataLineage_ID == source.DataLineage_ID))
        //                {
        //                    candidateChild.Sources.Add(source);
        //                }
        //            }

        //        }

        //    }

        //    if (!cachedList.Any(cache => cache.DataElement_NME == parent.DataElement_NME
        //                && cache.DataObject_NME == parent.DataObject_NME
        //                && cache.DataObjectField_NME == parent.DataObjectField_NME))
        //    {
        //        cachedList.Add(parent);
        //    }

        //    return parent.Sources;

        //}

        private List<LineageCreation> RecursiveSources(LineageCreation input, List<Lineage> allLineage)
        {
            var lcList = allLineage.Where(a => a.DataObject_NME.Trim().ToLower() == input.SourceObject_NME.Trim().ToLower() && 
            a.DataObjectField_NME.Trim().ToLower() == input.SourceField_NME.Trim().ToLower() && 
            a.DataElement_NME.Trim().ToLower() == input.SourceElement_NME.Trim().ToLower()).ToList();

            if(input.Sources == null)
            {
                input.Sources = new List<LineageCreation>();
            }

            foreach (Lineage l in lcList)
            {
                LineageCreation lc = input.Sources.FirstOrDefault(a => a.DataElement_NME == l.DataElement_NME && a.DataObject_NME == l.DataObject_NME && a.DataObjectField_NME == l.DataObjectField_NME);

                if (lc != null)
                {
                    lc.SourceElement_NME = l.SourceElement_NME;
                    lc.SourceField_NME = l.SourceField_NME;
                    lc.SourceObject_NME = l.SourceObject_NME;
                }
                else
                {
                    lc = new LineageCreation()
                    {
                        DataAsset_ID = l.DataAsset_ID,
                        Layer = input.Layer + 1,

                        Model_NME = l.Model_NME,

                        DataElement_NME = l.DataElement_NME,
                        DataElement_TYP = l.DataElement_TYP,

                        DataObject_NME = l.DataObject_NME,
                        DataObject_DSC = l.DataObject_DSC,
                        DataObjectCode_DSC = l.DataObjectCode_DSC,

                        DataObjectDetailType_VAL = l.DataObjectDetailType_VAL,
                        DataObjectField_NME = l.DataObjectField_NME,
                        DataObjectField_DSC = l.DataObjectField_DSC,

                        SourceElement_NME = l.SourceElement_NME,
                        SourceField_NME = l.SourceField_NME,
                        SourceObject_NME = l.SourceObject_NME,

                        Display_IND = l.Display_IND,
                        Sources = new List<LineageCreation>(),

                        DataLineage_ID = l.DataLineage_ID,
                        Transformation_TXT = l.Transformation_TXT,
                        Source_TXT = l.Source_TXT
                    };

                    input.Sources.Add(lc);
                }

                foreach(var source in RecursiveSources(lc, allLineage))
                {
                    if(!lc.Sources.Any(x => x.DataLineage_ID == source.DataLineage_ID))
                    {
                        lc.Sources.Add(source);
                    }
                }                
            }
           
            return input.Sources;
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
