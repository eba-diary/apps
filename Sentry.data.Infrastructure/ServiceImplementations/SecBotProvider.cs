using Sentry.data.Core;
using Sentry.data.Core.DTO.Security;
using System;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;
using Sentry.Common.Logging;
using System.Net;

namespace Sentry.data.Infrastructure.ServiceImplementations
{
    /// <summary>
    /// Implements the interface to administer AD security, including creating AD groups
    /// </summary>
    public class SecBotProvider : IAdSecurityAdminProvider
    {
        private readonly IRestClient restClient;

        /// <summary>
        /// Public constructor
        /// </summary>
        public SecBotProvider(IRestClient restClient)
        {
            this.restClient = restClient;
        }

        /// <summary>
        /// Makes an authenticated call to SecBot to create an AD group
        /// </summary>
        /// <param name="adSecurityGroupDto">Inputs to create the AD group</param>
        /// <exception cref="Exceptions.SecBotProviderException"></exception>
        public async Task CreateAdSecurityGroupAsync(AdSecurityGroupDto adSecurityGroupDto)
        {
            //Attempt to get an authentication "crumb" from SecBot Jenkins cluster
            var cookieJar = new CookieContainer();
            restClient.CookieContainer = cookieJar;
            restClient.PreAuthenticate = true;
            var authRequest = new RestRequest("crumbIssuer/api/json", Method.GET);
            var authResponse = await restClient.ExecuteGetTaskAsync<JenkinsCrumbResponse>(authRequest);
            if (!authResponse.IsSuccessful || authResponse.StatusCode != HttpStatusCode.OK)
            {
                Logger.Error("Could not retrieve an Authentication crumb from SecBot. See custom fields for additional details.", authResponse.ErrorException, new TextVariable("http_response_content", authResponse.Content), new TextVariable("http_status_code", authResponse.StatusCode.ToString()));
                throw new Exceptions.SecBotProviderException($"Could not retrieve an Authentication crumb from SecBot. Http Status = \"{authResponse.StatusCode}\"; Response Content = \"{authResponse.Content}\";", authResponse.ErrorException);
            }

            //create a request with all the parameters needed for the SecBot Jenkins job
            var request = new RestRequest("job/ActiveDirectory/job/Invoke-CreateDSCGroup/buildWithParameters", Method.POST)
                .AddParameter("SAIDAssetKey", adSecurityGroupDto.SaidAssetCode)
                .AddParameter("DSCName", adSecurityGroupDto.DatasetShortName)
                .AddParameter("DSCRole", adSecurityGroupDto.GroupType)
                .AddParameter("DSCEnv", adSecurityGroupDto.EnvironmentType)
                .AddHeader("Jenkins-Crumb", authResponse.Data.crumb);

            //execute the POST
            var response = await restClient.ExecutePostTaskAsync(request);

            //only successful if the status code == 201
            if (response.IsSuccessful && response.StatusCode == HttpStatusCode.Created)
            {
                var secBotJobUrl = response.Headers.FirstOrDefault(h => h.Name == "Location");
                Logger.Info($"SecBot job created at \"{secBotJobUrl}\"");
            }
            else
            {
                Logger.Error("SecBot request was not successful. See custom fields for additional details.", response.ErrorException, new TextVariable("http_response_content", response.Content), new TextVariable("http_status_code", response.StatusCode.ToString()));
                throw new Exceptions.SecBotProviderException($"SecBot request was not successful. Http Status = \"{response.StatusCode}\"; Response Content = \"{response.Content}\"", response.ErrorException);
            }
        }


#pragma warning disable IDE1006  // Naming Styles disabled, as this class is setup to match the format that Jenkins responds to
        public sealed class JenkinsCrumbResponse
        {
            public string _class { get; set; }
            public string crumb { get; set; }
            public string crumbRequestField { get; set; }
        }
#pragma warning restore IDE1006 // Naming Styles

    }
}
