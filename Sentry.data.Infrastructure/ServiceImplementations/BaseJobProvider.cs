using Sentry.data.Core;
using System;

namespace Sentry.data.Infrastructure
{
    public abstract class BaseJobProvider : IBaseJobProvider
    {
        protected RetrieverJob _job;
        protected readonly Lazy<IJobService> _jobService;

        protected BaseJobProvider(Lazy<IJobService> jobService)
        {
            _jobService = jobService;
        }

        public abstract void ConfigureProvider(RetrieverJob job);

        public abstract void Execute(RetrieverJob job);

        public abstract void Execute(RetrieverJob job, string filePath);
    }
}
