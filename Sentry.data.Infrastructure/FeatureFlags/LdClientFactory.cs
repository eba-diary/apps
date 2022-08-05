using LaunchDarkly.Logging;
using LaunchDarkly.Sdk.Server;
using LaunchDarkly.Sdk.Server.Integrations;
using System;
using Logger = Sentry.Common.Logging.Logger;

namespace Sentry.data.Infrastructure.FeatureFlags
{
    /// <summary>
    /// Class containing methods to instantiate the LaunchDarkly client class
    /// </summary>
    public static class LdClientFactory
    {

        //This method is responsible for building the LaunchDarkly client that will be used.
        //That client should be re-used for your entire app, so this method should only be called once.
        public static LdClient BuildLdClient()
        {
            var configBuilder = LaunchDarkly.Sdk.Server.Configuration.Builder(Configuration.Config.GetHostSetting("LaunchDarklyEnvironmentKey"))
                .Logging(LdLog4net.Adapter);

            var ldClient = bool.Parse(Configuration.Config.GetHostSetting("LaunchDarklyUseFileDataSource")) ? 
                BuildLdClientForFileDataSource(ref configBuilder) : 
                BuildLdClient(ref configBuilder);

            //if there's a problem initializing the LaunchDarkly client, log an error so a Spectre spy can alert on the problem - but don't stop the app from starting up
            if (!ldClient.Initialized)
            {
                Logger.Error("LaunchDarkly client failed to initialize",
                             new Exception(ldClient.DataSourceStatusProvider.Status.LastError.ToString()),
                             new Common.Logging.TextVariable("LaunchDarklyClientState", ldClient.DataSourceStatusProvider.Status.State.ToString()));
            }
            return ldClient;
        }

        /// <summary>
        /// Builds an LdClient that is configured to use the proxy servers to communicate with LaunchDarkly
        /// </summary>
        private static LdClient BuildLdClient(ref ConfigurationBuilder configBuilder)
        {
            //configure LD to use the proxy if necessary
            if (Convert.ToBoolean(Configuration.Config.GetHostSetting("UseProxy")))
            {
                configBuilder = configBuilder.Http(
                        Components.HttpConfiguration().Proxy(
                            new System.Net.WebProxy(new Uri(Configuration.Config.GetHostSetting("WebProxyUrl"))) { UseDefaultCredentials = true }));
            }
            return new LdClient(configBuilder.Build());
        }

        /// <summary>
        /// Build an LdClient that uses a local .json file rather than communicating with LaunchDarkly
        /// </summary>
        private static LdClient BuildLdClientForFileDataSource(ref ConfigurationBuilder configBuilder)
        {
            // combine BaseDirectory and RelativeSearchPath in order to accurately get the bin directory path for both web and console app projects
            string localFeatureFlagsFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
                                                                  AppDomain.CurrentDomain.RelativeSearchPath ?? "", 
                                                                  "LocalFeatureFlags.json");
            var fileSource = new FileDataSourceBuilder().FilePaths(localFeatureFlagsFile).AutoUpdate(true);

            configBuilder = configBuilder
                .DataSource(fileSource)
                .Events(Components.NoEvents);

            return new LdClient(configBuilder.Build());
        }
    }
}
