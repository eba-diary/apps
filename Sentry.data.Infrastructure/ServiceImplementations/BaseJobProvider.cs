using Sentry.data.Core;

namespace Sentry.data.Infrastructure
{
    public abstract class BaseJobProvider : IBaseJobProvider
    {
        protected RetrieverJob _job;

        public abstract void ConfigureProvider(RetrieverJob job);

        public abstract void Execute(RetrieverJob job);

        public abstract void Execute(RetrieverJob job, string filePath);
    }
}
