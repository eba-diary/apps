using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IJobService
    {
        JobHistory GetLastExecution(RetrieverJob job);
        Submission SaveSubmission(RetrieverJob job, string options);
        void RecordJobState(Submission submission, RetrieverJob job, string state);
        RetrieverJob FindBasicJob(RetrieverJob job);
    }
}
