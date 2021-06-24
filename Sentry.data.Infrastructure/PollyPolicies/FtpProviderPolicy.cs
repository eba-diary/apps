using Polly;
using Polly.Registry;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Core.Interfaces;
using System;
using System.Net;

namespace Sentry.data.Infrastructure.PollyPolicies
{
    public class FtpProviderPolicy : IPollyPolicy
    {
        private readonly IPolicyRegistry<string> registry;

        public FtpProviderPolicy(IPolicyRegistry<string> registry)
        {
            this.registry = registry;
        }
        public void Register()
        {
            this.registry.Add(PollyPolicyKeys.FtpProviderPolicy, BuildFtpProviderProviderPolicy());
        }

        public Policy BuildFtpProviderProviderPolicy()
        {
            var RetryPolicy = Policy
                .Handle<WebException>()
                .WaitAndRetry(
                    retryCount: 4,
                    sleepDurationProvider: retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (Exception, retryWaitDuration, context) =>
                    {
                        Logger.Info($@"
                            Wait {retryWaitDuration} then retry
                            {Exception.Message}
                        ");
                    });

            return RetryPolicy;
        }
    }
}
