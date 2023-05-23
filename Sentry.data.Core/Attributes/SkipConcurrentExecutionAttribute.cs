using Hangfire.Common;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using System;
using System.Text;

namespace Sentry.data.Core
{
    /// <summary>
    /// Attribute to skip a job execution if the same job is already running.
	/// Lock is at a grain of method name and parameters passed.
	/// Lock is performed by a distributed lock at the storage level.
    /// Mostly taken from: http://discuss.hangfire.io/t/job-reentrancy-avoidance-proposal/607
    /// </summary>
    public class SkipConcurrentExecutionAttribute : JobFilterAttribute, IServerFilter
	{
		private readonly int _timeoutInSeconds;
		private static ILogger<SkipConcurrentExecutionAttribute> _logger = Logging.LoggerFactory.CreateLogger<SkipConcurrentExecutionAttribute>();

		public SkipConcurrentExecutionAttribute(int timeoutInSeconds)
		{
			if (timeoutInSeconds < 0) throw new ArgumentException("Timeout argument value should be greater that zero.");

			_timeoutInSeconds = timeoutInSeconds;
		}


		public void OnPerforming(PerformingContext filterContext)
		{

			var resource = GetResource(filterContext.BackgroundJob.Job);

			var timeout = TimeSpan.FromSeconds(_timeoutInSeconds);

			try
			{
				var distributedLock = filterContext.Connection.AcquireDistributedLock(resource, timeout);
				filterContext.Items["DistributedLock"] = distributedLock;
			}
			catch (Exception)
			{
				filterContext.Canceled = true;
				_logger.LogWarning(($"Cancelling run for {resource} job, id: {filterContext.BackgroundJob.Id}"));
			}
		}

		public void OnPerformed(PerformedContext filterContext)
		{
			if (!filterContext.Items.ContainsKey("DistributedLock"))
			{
				throw new InvalidOperationException("Can not release a distributed lock: it was not acquired.");
			}

			var distributedLock = (IDisposable)filterContext.Items["DistributedLock"];
			distributedLock.Dispose();
		}

		internal virtual string GetResource(Job job)
		{
			StringBuilder argString = new StringBuilder();
			if (job.Args.Count > 0)
			{
                for (int i = 0; i < job.Args.Count; i++)
                {
					argString.Append(job.Args[i].ToString());
					if (i+1 != job.Args.Count)
                    {
						argString.Append(".");
					}
				}
			}

			var resource = String.Format(
								 "{0}.{1}.{2}",
								job.Type.FullName,
								job.Method.Name,
								argString.ToString());

			return resource;
		}
	}
}
