using Polly;
using Polly.Registry;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.Interfaces;
using System;

namespace Sentry.data.Infrastructure.PollyPolicies
{

    public class GenericHttpProviderPolicy : IPollyPolicy
    {
        private readonly IPolicyRegistry<string> registry;

        public GenericHttpProviderPolicy(IPolicyRegistry<string> registry)
        {
            this.registry = registry;
        }

        public void Register()
        {
            this.registry.Add(PollyPolicyKeys.GenericHttpProviderPolicy, BuildGenericHttpProviderPolicy());
        }

        public Policy BuildGenericHttpProviderPolicy()
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
