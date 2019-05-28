using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public static class JobExtensions
    {
        public static Submission ToSubmission(this RetrieverJob job)
        {
            return new Submission()
            {
                JobId = job,
                JobGuid = new Guid(),
                Created = DateTime.Now,
                Serialized_Job_Options = null
            };
        }
    }
}
