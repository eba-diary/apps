using Polly;
using Polly.Registry;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.PollyPolicies
{
    public class ApacheLivyProviderPolicy : IPollyPolicy
    {
        private readonly IPolicyRegistry<string> registry;

        public ApacheLivyProviderPolicy(IPolicyRegistry<string> registry)
        {
            this.registry = registry;
        }

        public void Register()
        {
            this.registry.Add(PollyPolicyKeys.ApacheLivyProviderAsyncPolicy, BuildApacheLivyProviderAsyncPolicy());
        }

        public AsyncPolicy BuildApacheLivyProviderAsyncPolicy()
        {
            var asyncRetryPolicy = Policy
                .Handle<System.Net.Http.HttpRequestException>()
                .Or<TaskCanceledException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt =>
                        TimeSpan.FromMilliseconds(500),
                    onRetry: (Exception, retryWaitDuration, context) =>
                    {
                        Logger.Info($@"
                            Wait {retryWaitDuration} then retry
                            {Exception.Message}
                        ");
                    });

            return asyncRetryPolicy;
        }

        //private void FallBackMethod()
        //{

        //}
    }
}