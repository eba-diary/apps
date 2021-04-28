﻿using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
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

        public void OnPerformed(PerformedContext filterContext)
        {
            if (filterContext.Exception == null || filterContext.ExceptionHandled)
            {
                RemoveFingerprint(filterContext.Connection, filterContext.Job);
            }
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
            var parameters = string.Empty;
            if (job?.Arguments != null)
            {
                parameters = string.Join(".", job.Arguments);
            }
            if (job?.Type == null || job.Method == null)
            {
                return string.Empty;
            }
            var payload = $"{job.Type.FullName}.{job.Method.Name}.{parameters}";
            var hash = SHA256.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload));
            var fingerprint = Convert.ToBase64String(hash);
            return fingerprint;
        }

        void IClientFilter.OnCreated(CreatedContext filterContext)
        {
            throw new NotSupportedException();
        }

        void IServerFilter.OnPerforming(PerformingContext filterContext)
        {
            throw new NotSupportedException();
        }
    }
}