using Sentry.Core;
using System;
using System.IO;

namespace Sentry.data.Core
{
    public abstract class DfsEnvironmentSource : DataSource
    {
        public override Uri CalcRelativeUri(RetrieverJob Job)
        {
            return new Uri(Path.Combine(BaseUri.AbsolutePath, Job.RelativeUri).ToString());
        }

        public override string GetDropPrefix(RetrieverJob Job)
        {
            throw new NotImplementedException();
        }

        public override void Validate(RetrieverJob job, ValidationResults validationResults)
        {
            throw new NotImplementedException();
        }
    }
}
