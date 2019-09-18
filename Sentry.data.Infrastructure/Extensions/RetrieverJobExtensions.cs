﻿using System.Collections.Generic;
using System.Linq;
using Sentry.data.Core;
using Sentry.Core;
using Sentry.Common.Logging;
using System;
using System.IO;

namespace Sentry.data.Infrastructure
{
    public static class RetrieverJobExtensions
    {
        public static List<RetrieverJob> FetchAllConfiguration(this IQueryable<RetrieverJob> query, IDatasetContext datasetContext)
        {
            /* Retrieve Config(s) and Schema(s) */
            
            var configs = datasetContext.DatasetFileConfigs.Where(x => query.Any(y => x.ConfigId == y.DatasetConfig.ConfigId));
            var schemas = datasetContext.DataElements.Where(x => configs.Any(y => x.DatasetFileConfig.ConfigId == y.ConfigId));
            schemas.FetchMany(x => x.DataElementDetails).ToFuture();
            schemas.FetchMany(x => x.DataObjects).ToFuture();
            query.Fetch(x => x.DatasetConfig).ThenFetchMany(x => x.Schemas).ToFuture();
            
            var jobs = query.Fetch(f => f.DatasetConfig).ThenFetch(x => x.ParentDataset).Fetch(f => f.DataSource).ToFuture();

            return jobs.ToList();
        }

        public static string GetTargetPath(this RetrieverJob basicJob, RetrieverJob executingJob)
        {
            string basepath;
            string targetPath = null;
            if (basicJob.DataSource.Is<DfsBasic>())
            {
                basepath = basicJob.GetUri().LocalPath + '\\';
            }
            else if (basicJob.DataSource.Is<S3Basic>())
            {
                basepath = basicJob.DataSource.GetDropPrefix(basicJob);
            }
            else
            {
                throw new NotImplementedException("Not Configured to determine target path for data source type");
            }

            if (executingJob.DataSource.Is<HTTPSSource>())
            {
                targetPath = $"{basepath}{executingJob.GetTargetFileName(Path.GetFileName(executingJob.GetUri().ToString()))}";
            }
            else
            {
                targetPath = Path.Combine(basepath, executingJob.GetTargetFileName(Path.GetFileName(executingJob.GetUri().ToString())));
            }

            return targetPath;
        }

        public static string SetupTempWorkSpace(this RetrieverJob job)
        {
            string tempFile = Path.Combine(Configuration.Config.GetHostSetting("GoldenEyeWorkDir"), "Jobs", job.Id.ToString(), (Guid.NewGuid().ToString() + ".txt"));

            //Create temp directory if exists
            Directory.CreateDirectory(Path.GetDirectoryName(tempFile));

            //Remove temp file if exists
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
            return tempFile;
        }
    }
}
