using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IRetrieverJobService
    {
        void RunRetrieverJob(int JobId, IJobCancellationToken token, string filePath = null);

        bool DisableJob(int JobId);

        void EnableJob(int JobId);

        void DeleteJob(int JobId);

    }
}
