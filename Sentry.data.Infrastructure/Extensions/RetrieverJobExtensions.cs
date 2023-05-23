using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using System;
using System.IO;

namespace Sentry.data.Infrastructure
{
    public static class RetrieverJobExtensions
    {
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

        public static string GetTargetPath(this DataFlowStep s3DropStep, RetrieverJob executingJob)
        {
            string targetPath;

            if (s3DropStep.DataAction_Type_Id != DataActionType.S3Drop && s3DropStep.DataAction_Type_Id != DataActionType.ProducerS3Drop)
            {
                throw new NotImplementedException("Only Configured to determine target path for S3DropSteps");
            }

            if (executingJob.DataSource.Is<HTTPSSource>())
            {
                targetPath = $"{s3DropStep.TriggerKey}{executingJob.GetTargetFileName(Path.GetFileName(executingJob.GetUri().ToString()))}";
            }
            else
            {
                throw new NotImplementedException("retrieverjobextensions-gettargetpath - Only configured to get file name from https sources");
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
