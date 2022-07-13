using System.Linq;
using Sentry.Core;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public static class DataSourceQueryExtensions
    {
        public static List<DataSource> FetchSecurityTree(this IQueryable<DataSource> query, IDatasetContext datasetContext)
        {
            var security = datasetContext.Security.Where(s => query.Any(d => s.SecurityId == d.Security.SecurityId));
            var tickets = datasetContext.SecurityTicket.Where(t => security.Any(s => t.ParentSecurity.SecurityId == s.SecurityId));
            tickets.FetchMany(x => x.AddedPermissions).ThenFetch(p => p.Permission).ToFuture();
            var tree = query.Fetch(d => d.Security).ThenFetchMany(s => s.Tickets).ToFuture();

            return tree.ToList();
        }
    }
}
