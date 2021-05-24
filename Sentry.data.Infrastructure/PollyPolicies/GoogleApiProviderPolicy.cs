using Polly;
using Polly.Registry;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.PollyPolicies
{
    public class GoogleApiProviderPolicy : IPollyPolicy
    {
        private readonly IPolicyRegistry<string> registry;

        public GoogleApiProviderPolicy(IPolicyRegistry<string> registry)
        {
            this.registry = registry;
        }

        public void Register()
        {
            this.registry.Add(PollyPolicyKeys.GoogleAPiProviderPolicy, BuildGoogleApiProviderPolicy());
        }

        public Policy BuildGoogleApiProviderPolicy()
        {
            var RetryPolicy = Policy
                .Handle<System.Net.Http.HttpRequestException>()
                .Or<RetrieverJobProcessingException>()
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
