using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    /// <summary>
    /// Apply this filter to a method or class to prevent concurrnet execution class or method with same parameter signature. 
    /// (i.e two RetrieverJobService.Run(24) method will not run concurrently)
    /// https://gist.github.com/odinserj/a8332a3f486773baa009
    /// </summary>
    public class DisableMultipleQueuedItemsFilter : JobFilterAttribute, IClientFilter, IServerFilter
    {
        private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan FingerprintTimeout = TimeSpan.FromHours(1);

        public void OnCreating(CreatingContext filterContext)
        {
            if (!AddFingerprintIfNotExists(filterContext.Connection, filterContext.Job))
            {
                filterContext.Canceled = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterContext"></param>
        public void OnPerformed(PerformedContext filterContext)
        {
            RemoveFingerprint(filterContext.Connection, filterContext.Job);
        }

        private static bool AddFingerprintIfNotExists(IStorageConnection connection, Job job)
        {
            using (connection.AcquireDistributedLock(GetFingerprintLockKey(job), LockTimeout))
            {
                var fingerprint = connection.GetAllEntriesFromHash(GetFingerprintKey(job));

                DateTimeOffset timestamp;

                if (fingerprint != null &&
                    fingerprint.ContainsKey("Timestamp") &&
                    DateTimeOffset.TryParse(fingerprint["Timestamp"], null, DateTimeStyles.RoundtripKind, out timestamp) &&
                    DateTimeOffset.UtcNow <= timestamp.Add(FingerprintTimeout))
                {
                    // Actual fingerprint found, returning.
                    return false;
                }

                // Fingerprint does not exist, it is invalid (no `Timestamp` key),
                // or it is not actual (timeout expired).
                connection.SetRangeInHash(GetFingerprintKey(job), new Dictionary<string, string>
            {
                { "Timestamp", DateTimeOffset.UtcNow.ToString("o") }
            });

                return true;
            }
        }

        private static void RemoveFingerprint(IStorageConnection connection, Job job)
        {
            using (connection.AcquireDistributedLock(GetFingerprintLockKey(job), LockTimeout))
            using (var transaction = connection.CreateWriteTransaction())
            {
                transaction.RemoveHash(GetFingerprintKey(job));
                transaction.Commit();
            }
        }

        private static string GetFingerprintLockKey(Job job)
        {
            return String.Format("{0}:lock", GetFingerprintKey(job));
        }

        private static string GetFingerprintKey(Job job)
        {
            return String.Format("fingerprint:{0}", GetFingerprint(job));
        }

        private static string GetFingerprint(Job job)
        {
            string parameters = string.Empty;
            if (job.Arguments != null)
            {
                parameters = string.Join(".", job.Arguments);
            }
            if (job.Type == null || job.Method == null)
            {
                return string.Empty;
            }
            var fingerprint = String.Format(
                "{0}.{1}.{2}",
                job.Type.FullName,
                job.Method.Name, parameters);

            return fingerprint;
        }

        void IClientFilter.OnCreated(CreatedContext filterContext)
        {
        }

        void IServerFilter.OnPerforming(PerformingContext filterContext)
        {
        }
    }
}
