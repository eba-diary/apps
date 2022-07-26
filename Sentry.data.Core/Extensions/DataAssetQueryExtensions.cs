using System;
using System.Collections.Generic;
using System.Linq;
using Sentry.Core;

namespace Sentry.data.Core
{
    public static class DataAssetQueryExtensions
    {

        public static List<DataAsset> FetchSecurityTree(this IQueryable<DataAsset> query, IDatasetContext datasetContext)
        {
            var security = datasetContext.Security.Where(s => query.Any(d => s.SecurityId == d.Security.SecurityId));
            var tickets = datasetContext.SecurityTicket.Where(t => security.Any(s => t.ParentSecurity.SecurityId == s.SecurityId));
            tickets.FetchMany(x => x.AddedPermissions).ThenFetch(p => p.Permission).ToFuture();
            var tree = query.Fetch(d => d.Security).ThenFetchMany(s => s.Tickets).ToFuture();

            return tree.ToList();
        }

    }
}
