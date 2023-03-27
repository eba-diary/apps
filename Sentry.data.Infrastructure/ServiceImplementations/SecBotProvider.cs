using Sentry.data.Core;
using Sentry.data.Core.DTO.Security;
using System;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;
using Sentry.Common.Logging;
using System.Net;
using Polly;

namespace Sentry.data.Infrastructure.ServiceImplementations
{
    /// <summary>
    /// Implements the interface to administer AD security, including creating AD groups
    /// </summary>
    public class SecBotProvider : IAdSecurityAdminProvider
    {
        private readonly RestClient restClient;

        /// <summary>
        /// Public constructor
        /// </summary>
        public SecBotProvider(RestClient restClient)
        {
            this.restClient = restClient;
        }

        /// <summary>
        /// Makes an authenticated call to SecBot to create an AD group, 
        /// and then waits for SecBot to complete processing
        /// </summary>
        /// <param name="adSecurityGroupDto">Inputs to create the AD group</param>
        /// <exception cref="Exceptions.SecBotProviderException"></exception>
        public async Task CreateAdSecurityGroupAsync(AdSecurityGroupDto adSecurityGroupDto)
        {
            //Attempt to get an authentication "crumb" from SecBot Jenkins cluster
            var cookieJar = new CookieContainer();
            restClient.Options.CookieContainer = cookieJar;
            restClient.Options.PreAuthenticate = true;
            var authRequest = new RestRequest("crumbIssuer/api/json", Method.Get);
            var authResponse = await restClient.ExecuteGetAsync<JenkinsCrumbResponse>(authRequest);
            if (!authResponse.IsSuccessful || authResponse.StatusCode != HttpStatusCode.OK)
            {
                Logger.Error("Could not retrieve an Authentication crumb from SecBot. See custom fields for additional details.", authResponse.ErrorException, new TextVariable("http_response_content", authResponse.Content), new TextVariable("http_status_code", authResponse.StatusCode.ToString()));
                throw new Exceptions.SecBotProviderException($"Could not retrieve an Authentication crumb from SecBot. Http Status = \"{authResponse.StatusCode}\"; Response Content = \"{authResponse.Content}\";", authResponse.ErrorException);
            }

            //create a request with all the parameters needed for the SecBot Jenkins job
            var request = new RestRequest("job/ActiveDirectory/job/Invoke-CreateDSCGroup/buildWithParameters", Method.Post)
                .AddParameter("SAIDAssetKey", adSecurityGroupDto.SaidAssetCode)
                .AddParameter("DSCName", adSecurityGroupDto.DatasetShortName)
                .AddParameter("DSCRole", adSecurityGroupDto.GroupType)
                .AddParameter("DSCEnv", adSecurityGroupDto.EnvironmentType)
                .AddHeader("Jenkins-Crumb", authResponse.Data.crumb);

            //execute the POST
            var response = await restClient.ExecutePostAsync(request);

            //throw exception if not successful
            if (!response.IsSuccessful || response.StatusCode != HttpStatusCode.Created)
            {
                Logger.Error("SecBot buildWithParameters was not successful. See custom fields for additional details.", new TextVariable("http_response_content", response.Content), new TextVariable("http_status_code", response.StatusCode.ToString()));
                throw new Exceptions.SecBotProviderException($"SecBot request was not successful. Http Status = \"{response.StatusCode}\"; Response Content = \"{response.Content}\"", response.ErrorException);
            }

            //now wait for the SecBot queue to process
            var secBotQueueUrl = response.Headers.FirstOrDefault(h => h.Name == "Location").Value.ToString();
            Logger.Info($"SecBot queued at \"{secBotQueueUrl}\"");
            var secBotJobUrl = await PollForSecBotQueueStatus(authResponse.Data.crumb, secBotQueueUrl);

            //then wait for the SecBot job to complete
            await PollForSecBotJobStatus(authResponse.Data.crumb, secBotJobUrl);
        }

        /// <summary>
        ///The timespan to pause between retries 
        /// After failure, will wait for:
        ///  2 ^ 1 = 2 seconds then
        ///  2 ^ 2 = 4 seconds then
        ///  2 ^ 3 = 8 seconds then
        ///  2 ^ 4 = 16 seconds then
        ///  2 ^ 5 = 32 seconds
        /// </summary>
        /// <param name="retryAttempt">How many attempts have already occurred</param>
        /// <remarks>Declared as virtual so it can be overriden in Unit Tests</remarks>
        /// <returns>How long to wait before retrying</returns>
        internal virtual TimeSpan RetryPauseTimespan(int retryAttempt) => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));

        /// <summary>
        /// Wait for SecBot to process the initial request (queue) and generate an actual job
        /// </summary>
        /// <param name="jenkinsAuthCrumb">A token to authenticate with</param>
        /// <param name="secBotQueueUrl">The URL to the queued request, that we'll check</param>
        /// <returns>The URL to the actual SecBot job that gets generated</returns>
        /// <exception cref="Exceptions.SecBotProviderException"></exception>
        internal async Task<string> PollForSecBotQueueStatus(string jenkinsAuthCrumb, string secBotQueueUrl)
        {
            var queueRequest = new RestRequest(new Uri(new Uri(secBotQueueUrl), "api/json"), Method.Get)
                                .AddHeader("Jenkins-Crumb", jenkinsAuthCrumb);

            RestResponse<JenkinsQueueResponse> queueResponse;
            var jenkinsJobUrl = "";
            await Policy
                .Handle<Exceptions.SecBotProviderException>(e => e.Message.Contains("pending"))
                .WaitAndRetryAsync(5, RetryPauseTimespan,
                (exception, timeSpan, retryCount, context) =>
                {
                    Logger.Warn($"SecBot queue is still executing. Attempt {retryCount}/5. Will wait {timeSpan.TotalSeconds} seconds before checking status again.");
                })
                .ExecuteAsync(async () =>
                {
                    queueResponse = await restClient.ExecuteGetAsync<JenkinsQueueResponse>(queueRequest);
                    if (queueResponse.IsSuccessful && queueResponse.StatusCode == HttpStatusCode.OK)
                    {
                        if (queueResponse.Data.executable != null && queueResponse.Data.executable.url != null)
                        {
                            Logger.Info($"SecBot job created at \"{queueResponse.Data.executable.url}\"");
                            jenkinsJobUrl = queueResponse.Data.executable.url;
                        }
                        else
                        {
                            throw new Exceptions.SecBotProviderException($"SecBot queue \"{secBotQueueUrl}\" is still pending.");
                        }
                    }
                    else
                    {
                        Logger.Error("Couldn't retrieve the status of SecBot job. See custom fields for additional details.", new TextVariable("http_response_content", queueResponse.Content), new TextVariable("http_status_code", queueResponse.StatusCode.ToString()));
                        throw new Exceptions.SecBotProviderException($"Couldn't retrieve the status of SecBot job. Http Status = \"{queueResponse.StatusCode}\"; Response Content = \"{queueResponse.Content}\"", queueResponse.ErrorException);                    }
                });

            return jenkinsJobUrl;
        }

        /// <summary>
        /// It always takes the SecBot job a while to run; so pause for a few seconds before requesting the status
        /// </summary>
        /// <remarks>Declared as virtual so it can be overriden in Unit Tests</remarks>
        /// <returns>How long to wait before asking SecBot for the status initially</returns>
        internal virtual TimeSpan SecBotInitialPauseTimespan() => TimeSpan.FromSeconds(30);

        /// <summary>
        /// Wait for SecBot to process the actual job
        /// </summary>
        /// <param name="jenkinsAuthCrumb">A token to authenticate with</param>
        /// <param name="secBotJobUrl">The URL to the job that we'll check</param>
        /// <exception cref="Exceptions.SecBotProviderException"></exception>
        internal async Task PollForSecBotJobStatus(string jenkinsAuthCrumb, string secBotJobUrl)
        {
            var statusRequest = new RestRequest(new Uri(new Uri(secBotJobUrl), "api/json"), Method.Get)
                                            .AddHeader("Jenkins-Crumb", jenkinsAuthCrumb);

            //It always takes the SecBot job a while to run; so pause for a few seconds before requesting the status
            await Task.Delay(SecBotInitialPauseTimespan());

            // Define a Polly Retry Policy
            await Policy
                .Handle<Exceptions.SecBotProviderException>(e => e.Message.Contains("pending"))
                .WaitAndRetryAsync(5, RetryPauseTimespan,
                (exception, timeSpan, retryCount, context) =>
                {
                    Logger.Warn($"SecBot job is still executing. Attempt {retryCount}/5. Will wait {timeSpan.TotalSeconds} seconds before checking status again.");
                })
                .ExecuteAsync(async () =>
                {
                    var statusResponse = await restClient.ExecuteGetAsync<JenkinsJobStatusResponse>(statusRequest);
                    if (statusResponse.IsSuccessful && statusResponse.StatusCode == HttpStatusCode.OK)
                    {
                        if (statusResponse.Data.result == "SUCCESS")
                        {
                            Logger.Info($"SecBot job \"{secBotJobUrl}\" completed successfully.");
                        }
                        else if (statusResponse.Data.result == "FAILURE")
                        {
                            Logger.Error($"SecBot job \"{secBotJobUrl}\" failed.");
                            throw new Exceptions.SecBotProviderException($"SecBot job \"{secBotJobUrl}\" failed.");
                        }
                        else
                        {
                            throw new Exceptions.SecBotProviderException($"SecBot job \"{secBotJobUrl}\" is still pending.");
                        }
                    }
                    else
                    {
                        Logger.Error("Couldn't retrieve the status of SecBot job. See custom fields for additional details.", new TextVariable("http_response_content", statusResponse.Content), new TextVariable("http_status_code", statusResponse.StatusCode.ToString()));
                        throw new Exceptions.SecBotProviderException($"Couldn't retrieve the status of SecBot job. Http Status = \"{statusResponse.StatusCode}\"; Response Content = \"{statusResponse.Content}\"", statusResponse.ErrorException);
                    }
                });
        }


        #region "SecBot API Types"
#pragma warning disable IDE1006  // Naming Styles disabled, as these classes are setup to match the format that Jenkins responds to
        public sealed class JenkinsCrumbResponse
        {
            public string _class { get; set; }
            public string crumb { get; set; }
            public string crumbRequestField { get; set; }
        }

        public class JenkinsQueueResponse
        {
            public string _class { get; set; }
            public bool blocked { get; set; }
            public bool buildable { get; set; }
            public int id { get; set; }
            public long inQueueSince { get; set; }
            public string _params { get; set; }
            public bool stuck { get; set; }
            public string url { get; set; }
            public object why { get; set; }
            public bool cancelled { get; set; }
            public Executable executable { get; set; }
        }

        public class Executable
        {
            public string _class { get; set; }
            public int number { get; set; }
            public string url { get; set; }
        }

        public class JenkinsJobStatusResponse
        {
            public string _class { get; set; }
            public bool building { get; set; }
            public object description { get; set; }
            public string displayName { get; set; }
            public int duration { get; set; }
            public int estimatedDuration { get; set; }
            public object executor { get; set; }
            public string fullDisplayName { get; set; }
            public string id { get; set; }
            public bool keepLog { get; set; }
            public int number { get; set; }
            public int queueId { get; set; }
            public string result { get; set; }
            public long timestamp { get; set; }
            public string url { get; set; }
            public object nextBuild { get; set; }
        }

#pragma warning restore IDE1006 // Naming Styles
        #endregion
    }
}
