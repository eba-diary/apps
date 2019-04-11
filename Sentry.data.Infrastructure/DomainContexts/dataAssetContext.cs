using System.Linq;
using NHibernate;
using NHibernate.Linq;
using Sentry.Core;
using Sentry.NHibernate;
using Sentry.data.Core;
using System;
using System.Web;
using System.Collections.Generic;

namespace Sentry.data.Infrastructure
{
    [Obsolete("Merge everything here into a common DomainContext")]

    public class dataAssetContext : NHWritableDomainContext, IDataAssetContext
    {

        public dataAssetContext(ISession session) : base(session)
        {
            NHQueryableExtensionProvider.RegisterQueryableExtensionsProvider<dataAssetContext>();
        }

        public IQueryable<DomainUser> Users
        {
            get
            {
                return null;
            }
        }

        public IQueryable<DataAsset> DataAssets
        {
            get
            {
                return Query<DataAsset>().Cacheable();
            }
        }

        public IList<DataAsset> GetDataAssets()
        {
            return Query<DataAsset>().Cacheable().OrderBy(x => x.Name).ToList();
        }
        public DataAsset GetDataAsset(int id)
        {
            DataAsset da = Query<DataAsset>().Cacheable().FirstOrDefault(x => x.Id == id);
            return da;
        }

        public DataAsset GetDataAsset(string assetName)
        {
            DataAsset da = Query<DataAsset>().Cacheable().FirstOrDefault(x => x.Name == assetName);
            return da;
        }

        public IEnumerable<Notification> GetAssetNotificationsByDataAssetId(int id)
        {
            return Query<Notification>().Where(w => w.ParentObject == id && w.NotificationType == GlobalConstants.Notifications.DATAASSET_TYPE).ToList();
        }
        public IEnumerable<Notification> GetAllAssetNotifications()
        {
            return Query<Notification>().ToList();
        }
        public Notification GetAssetNotificationByID(int id)
        {
            return Query<Notification>().Where(w => w.NotificationId == id).First();
        }

        private IQueryable<Lineage> QueryCreator(Boolean forLineage, int? DataAsset_ID, String dataElementCode = "", String _DataElement_NME = "", String _DataObject_NME = "", String _DataObjectField_NME = "", String _Line_CDE = "")
        {
            string DataElement_NME = HttpUtility.UrlDecode(_DataElement_NME);
            string DataObject_NME = HttpUtility.UrlDecode(_DataObject_NME);
            string DataObjectField_NME = HttpUtility.UrlDecode(_DataObjectField_NME);
            string Line_CDE = HttpUtility.UrlDecode(_Line_CDE);

            var rawQuery = Query<Lineage>();

            if(!string.IsNullOrWhiteSpace(dataElementCode))
            {
                rawQuery = rawQuery.Where(x => x.DataElement_TYP == GlobalConstants.DataElementDescription.BUSINESS_TERM);
            }
            if (DataAsset_ID != null)
            {
                rawQuery = rawQuery.Where(c => c.DataAsset_ID == DataAsset_ID);
            }
            if (!string.IsNullOrWhiteSpace(Line_CDE))
            {
                rawQuery = rawQuery.Where(c => c.Line_CDE == Line_CDE);
            }

            //This is for the Lineage being returned to the Frame Below on the Lineage Page
            if(forLineage)
            {
                if (!string.IsNullOrWhiteSpace(DataElement_NME) && DataElement_NME != "null")
                {
                    rawQuery = rawQuery.Where(c => c.DataElement_NME == DataElement_NME);
                }
                if (!string.IsNullOrWhiteSpace(DataObject_NME) && DataObject_NME != "null")
                {
                    rawQuery = rawQuery.Where(d => d.DataObject_NME == DataObject_NME);
                }
            }
            //This is for the Drop Down Boxes at the Top of the Lineage Page
            else
            {
                if (!string.IsNullOrWhiteSpace(DataElement_NME) && DataElement_NME != "null")
                {
                    rawQuery = rawQuery.Where(c => c.SourceElement_NME == DataElement_NME);
                }
                if (!string.IsNullOrWhiteSpace(DataObject_NME) && DataObject_NME != "null")
                {
                    rawQuery = rawQuery.Where(d => d.SourceObject_NME == DataObject_NME);
                }
            }

            rawQuery = BusinessTermPredicate(forLineage, rawQuery, DataObjectField_NME, DataAsset_ID, Line_CDE);        

            return rawQuery;
        }

        private IQueryable<Lineage> BusinessTermPredicate(Boolean forLineage, IQueryable<Lineage> rawQuery, String DataObjectField_NME, int? DataAsset_ID, String Line_CDE = "")
        {
            if (!string.IsNullOrWhiteSpace(DataObjectField_NME) && DataObjectField_NME != "null")
            {
                var predicate = PredicateBuilder.False<Lineage>();

                var BusinessTermSources = Query<Lineage>()
                    .Where(x => x.DataElement_TYP == GlobalConstants.DataElementDescription.BUSINESS_TERM)
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

                    if (forLineage)
                    {
                        predicate = predicate.Or(p => p.DataObjectField_NME == temp);
                    }
                    else
                    {
                        predicate = predicate.Or(p => p.SourceField_NME == temp);
                    }
                }

                rawQuery = rawQuery.Where(predicate);
            }

            return rawQuery;
        }

        public List<String> BusinessTerms(string dataElementCode, int? DataAsset_ID, String DataElement_NME = "", String DataObject_NME = "", String DataObjectField_NME = "", String Line_CDE = "")
        {
            return QueryCreator(false, DataAsset_ID, dataElementCode,  DataElement_NME, DataObject_NME, "", Line_CDE).Select(x => x.DataElement_NME).ToList();
        }

        public List<String> ConsumptionLayers(string dataElementCode, int? DataAsset_ID, String DataElement_NME = "", String DataObject_NME = "", String DataObjectField_NME = "", String Line_CDE = "")
        {
            return QueryCreator(false, DataAsset_ID, dataElementCode, "", DataObject_NME, DataObjectField_NME, Line_CDE).Select(x => x.SourceElement_NME).ToList();
        }

        public List<String> LineageTables(string dataElementCode, int? DataAsset_ID, String DataElement_NME = "", String DataObject_NME = "", String DataObjectField_NME = "", String Line_CDE = "")
        {
            return QueryCreator(false, DataAsset_ID, dataElementCode, DataElement_NME, "", DataObjectField_NME, Line_CDE).Select(x => x.SourceObject_NME).ToList();
        }

        public List<String> BusinessTermDescription(string dataElementCode, int? DataAsset_ID, string DataObjectField_NME, String Line_CDE = "")
        {
            return QueryCreator(false, DataAsset_ID, dataElementCode, "", "", DataObjectField_NME, Line_CDE).Select(x => x.BusTerm_DSC).ToList();
        }

        public Lineage Description(int? DataAsset_ID, string _DataObject_NME, string _DataObjectField_NME, String _Line_CDE = "")
        {
            string DataObject_NME = HttpUtility.UrlDecode(_DataObject_NME);
            string DataObjectField_NME = HttpUtility.UrlDecode(_DataObjectField_NME);

            Lineage l = new Lineage();

            try
            {
                var dofDescription = Query<DataObjectField>()
                     .Where(a => a.DataObject.DataElement.DataElement_NME == "SERA PL.dm1")
                     .Where(b => DataObject_NME == b.DataObject.DataObject_NME)
                     .Where(c => DataObjectField_NME == c.DataObjectField_NME);

                l.DataObjectField_DSC = dofDescription.FirstOrDefault().DataObjectField_DSC;
            }
            catch
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
            catch
            {
                //There was no description.
                //Return a null Lineage Object
            }
            return l;
        }


        public List<LineageCreation> Lineage(string dataElementCode, int? DataAsset_ID, String DataElement_NME = "", String DataObject_NME = "", String DataObjectField_NME = "", String Line_CDE = "")
        {
            IQueryable<Lineage> rawQuery = QueryCreator(true, DataAsset_ID, "", DataElement_NME, DataObject_NME, DataObjectField_NME, Line_CDE);

            List<LineageCreation> masterList = new List<LineageCreation>();

            List<Lineage> allLineage = Query<Lineage>().ToList();

            foreach (Lineage l in rawQuery)
            {
                LineageCreation lc = masterList.FirstOrDefault(a => a.DataElement_NME == l.DataElement_NME && a.DataObject_NME == l.DataObject_NME && a.DataObjectField_NME == l.DataObjectField_NME);

                if (lc != null)
                {
                    lc.SourceElement_NME = l.SourceElement_NME;
                    lc.SourceField_NME = l.SourceField_NME;
                    lc.SourceObject_NME = l.SourceObject_NME;
                }
                else
                {
                    lc = new LineageCreation(l, 0);

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

        private List<LineageCreation> RecursiveSources(LineageCreation input, List<Lineage> allLineage)
        {
            var lcList = allLineage.Where(a => a.DataObject_NME.Trim().ToLower() == input.SourceObject_NME.Trim().ToLower() &&
            a.DataObjectField_NME.Trim().ToLower() == input.SourceField_NME.Trim().ToLower() &&
            a.DataElement_NME.Trim().ToLower() == input.SourceElement_NME.Trim().ToLower()).ToList();

            if (input.Sources == null)
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
                    lc = new LineageCreation(l, input.Layer + 1);
                    input.Sources.Add(lc);
                }

                foreach (var source in RecursiveSources(lc, allLineage))
                {
                    if (!lc.Sources.Any(x => x.DataLineage_ID == source.DataLineage_ID))
                    {
                        lc.Sources.Add(source);
                    }
                }
            }

            return input.Sources;
        }


    }
}
