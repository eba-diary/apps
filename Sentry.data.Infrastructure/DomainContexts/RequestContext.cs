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
    class requestContext : NHWritableDomainContext, IRequestContext
    {
        public requestContext(ISession session) : base(session)
        {
            NHQueryableExtensionProvider.RegisterQueryableExtensionsProvider<requestContext>();
        }

        public IQueryable<DomainUser> Users
        {
            get
            {
                return null;
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
