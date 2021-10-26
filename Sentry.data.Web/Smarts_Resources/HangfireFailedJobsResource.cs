using Hangfire;
using Sentry.Smarts.Resource;

namespace Sentry.data.Web.SmartsResources
{
    /// <summary>
    /// This custom Smarts resource will show degraded if any Hangfire jobs are sitting in a "Failed" status
    /// </summary>
    public class HangfireFailedJobsResource : CustomResource
    {
        public override void RequestResourceStatus()
        {
            int failedCount = JobStorage.Current.GetMonitoringApi().FailedJobs(0, 100000).Count;

            if (failedCount > 0)
            {
                SetStatus(ResourceStatus.Degraded, $"{failedCount} Hangfire jobs have failed.");
            }
            else
            {
                SetStatus(ResourceStatus.Up);
            }
        }
    }
}