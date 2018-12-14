using Hangfire.Client;
using Hangfire.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    class ExactlyOnceAttribute : JobFilterAttribute, IClientFilter
    {
        public void OnCreating(CreatingContext filterContext)
        {
            var entries = filterContext.Connection.GetAllEntriesFromHash(GetJobKey(filterContext.Job));
            if (entries != null && entries.ContainsKey("jobId"))
            {
                // this job was already created once, cancel creation
                filterContext.Canceled = true;
            }
        }

        public void OnCreated(CreatedContext filterContext)
        {
            if (!filterContext.Canceled)
            {
                // job created, mark it as such
                filterContext.Connection.SetRangeInHash(GetJobKey(filterContext.Job),
                    new[] { new KeyValuePair<string, string>("jobId", filterContext.BackgroundJob.Id) });
            }
        }

        private static string GetJobKey(Hangfire.Common.Job job)
        {
            using (var sha512 = SHA512.Create())
            {
                return Convert.ToBase64String(sha512.ComputeHash(Encoding.UTF8.GetBytes("execute-once:" + JsonConvert.SerializeObject(job))));
            }
        }
    }
}
