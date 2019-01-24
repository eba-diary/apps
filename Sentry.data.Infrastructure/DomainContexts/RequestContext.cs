using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.NHibernate;
using Sentry.data.Core;
using NHibernate;

namespace Sentry.data.Infrastructure
{
    public class RequestContext : NHWritableDomainContext, IRequestContext
    {
        public RequestContext(ISession session) : base(session)
        {
            NHQueryableExtensionProvider.RegisterQueryableExtensionsProvider<RequestContext>();
        }

        public IQueryable<DataSource> DataSource
        {
            get
            {
                return Query<DataSource>();
            }
        }

        public IQueryable<RetrieverJob> RetrieverJob
        {
            get
            {
                return Query<RetrieverJob>();
            }
        }

        public IQueryable<AuthenticationType> AuthenticationType
        {
            get
            {
                return Query<AuthenticationType>();
            }
        }

        public IQueryable<DomainUser> Users
        {
            get
            {
                return null;
            }
        }

        public IQueryable<DataSourceType> DataSourceTypes
        {
            get
            {
                return Query<DataSourceType>();
            }
        }

        public IQueryable<ApplicationConfiguration> ApplicaitonConfigurations
        {
            get
            {
                return Query<ApplicationConfiguration>();
            }
        }

        public IList<RTSourceTypes> GetSourceTypes()
        {
            return Query<RTSourceTypes>().ToList();
        }

        public IList<RTRequest> GetEnabledRequests()
        {
            List<RTRequest> list = Query<RTRequest>().Where(x => x.IsEnabled).ToList();
                       

            //if (Query<RTRequest>().Where(x => !x.IsEnabled).Count() > 0)
            //{
            //    return Query<RTRequest>().Where(x => !x.IsEnabled).ToList();
            //}
            //else
            //{
            //    IList<RTRequest> list = new List<RTRequest>();
            //    return list;
            //}

            //return list.Where(x => x.IsEnabled).ToList();
            return list;
        }

        public RTRequest GetRequest(int id)
        {
            RTRequest request = Query<RTRequest>().Where(x => x.Id == id).FirstOrDefault();

            return request;
        }


    }
}
