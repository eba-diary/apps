using System;
using System.Collections.Generic;
using System.Linq;
using Sentry.Core;


namespace Sentry.data.Core
{
    public static class DatasetQueryExtensions
    {


        public static List<Dataset> FetchSecurityTree(this IQueryable<Dataset> query, IDatasetContext datasetContext)
        {
            var security = datasetContext.Security.Where(s => query.Any(d => s.SecurityId == d.Security.SecurityId));
            var tickets = datasetContext.SecurityTicket.Where(t => security.Any(s => t.ParentSecurity.SecurityId == s.SecurityId));
            tickets.FetchMany(x => x.AddedPermissions).ThenFetch(p => p.Permission).ToFuture();
            tickets.FetchMany(x => x.RemovedPermissions).ThenFetch(p => p.Permission).ToFuture();
            var tree = query.Fetch(d => d.Security).ThenFetchMany(s => s.Tickets).ToFuture();

            return tree.ToList();
        }

        public static List<DatasetFileConfig> FetchSecurityTree(this IQueryable<DatasetFileConfig> query, IDatasetContext datasetContext)
        {

            var datasets = datasetContext.Datasets.Where(c => query.Any(d => c.DatasetId == d.ParentDataset.DatasetId));
            var security = datasetContext.Security.Where(s => datasets.Any(d => s.SecurityId == d.Security.SecurityId));
            var tickets = datasetContext.SecurityTicket.Where(t => security.Any(s => t.ParentSecurity.SecurityId == s.SecurityId));
            tickets.FetchMany(x => x.AddedPermissions).ThenFetch(p => p.Permission).ToFuture();

            var tree = query.Fetch(d => d.ParentDataset).ThenFetch(x=> x.Security).ThenFetchMany(s => s.Tickets).ToFuture();

            return tree.ToList();
        }

        public static List<Dataset> FetchAllChildren(this IQueryable<Dataset> query, IDatasetContext datasetContext)
        {

            //datasetFileConfigs
            var configs = datasetContext.DatasetFileConfigs.Where(x=> query.Any(y=> x.ParentDataset.DatasetId == y.DatasetId));
            configs.Fetch(x=> x.DatasetScopeType).ToFuture();
            configs.Fetch(x => x.Schema).ToFuture();
            query.FetchMany(d => d.DatasetFileConfigs).ToFuture();            

            //security
            var security = datasetContext.Security.Where(s => query.Any(d=> s.SecurityId == d.Security.SecurityId));
            var tickets = datasetContext.SecurityTicket.Where(t => security.Any(s => t.ParentSecurity.SecurityId == s.SecurityId));
            tickets.FetchMany(x => x.AddedPermissions).ThenFetch(p=> p.Permission).ToFuture();
            query.Fetch(d => d.Security).ThenFetchMany(s => s.Tickets).ToFuture();

            //all other
            query.FetchMany(d => d.DatasetCategories).ToFuture();
            query.FetchMany(d => d.Tags).ToFuture();
            query.FetchMany(d => d.BusinessUnits).ToFuture();
            query.FetchMany(d => d.DatasetFunctions).ToFuture();
            
            var temp = query.FetchMany(d => d.Favorities).ToFuture(); //create a variable so we can toList it...can't do a toList on teh query object.

            return temp.ToList();
        }

    }
}
