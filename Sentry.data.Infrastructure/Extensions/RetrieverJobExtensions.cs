using Sentry.data.Core;
using System;
using System.IO;

namespace Sentry.data.Infrastructure
{
    public static class RetrieverJobExtensions
    {
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
