using NHibernate.Linq;
using Sentry.data.Core;
using Sentry.NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;

namespace Sentry.data.Infrastructure
{
    public class ReportContext : NHWritableDomainContext, IReportContext
    {
        public ReportContext(ISession session) : base(session)
        {
            NHQueryableExtensionProvider.RegisterQueryableExtensionsProvider<ReportContext>();
        }

        public IQueryable<EventType> EventTypes
        {
            get
            {
                //TODO: Revisit for solution to filter based on user (i.e. Admins can see all eventtypes)
                return Query<EventType>().Cacheable();
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
                return Query<Dataset>().Where(x => x.DatasetType == "RPT").Cacheable();
            }
        }

        public IQueryable<Category> Categories
        {
            get
            {
                return Query<Category>().Where(w => w.ObjectType == "RPT").Cacheable();  //QueryCacheRegion.MediumTerm
            }
        }

        public IQueryable<DatasetScopeType> DatasetScopeTypes
        {
            get
            {
                return Query<DatasetScopeType>().Cacheable();
            }
        }

        public IQueryable<FileExtension> FileExtensions
        {
            get
            {
                return Query<FileExtension>().Cacheable();
            }
        }


        public int GetReportCount()
        {
            return Datasets.Cacheable().Count();
        }
    }
}
