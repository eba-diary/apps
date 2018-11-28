using Sentry.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IReportContext : IWritableDomainContext
    {
        IQueryable<EventType> EventTypes { get; }
        IQueryable<Status> EventStatus { get; }
        IQueryable<Dataset> Datasets { get; }
        IQueryable<Category> Categories { get; }
        IQueryable<DatasetScopeType> DatasetScopeTypes { get; }
        IQueryable<FileExtension> FileExtensions { get; }

        int GetReportCount();
    }
}
