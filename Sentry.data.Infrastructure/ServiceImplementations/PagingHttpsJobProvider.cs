using Amazon.Auth.AccessControlPolicy;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHibernate.Dialect;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Infrastructure
{
    public class PagingHttpsJobProvider : IBaseJobProvider
    {
        #region Fields
        private readonly IDatasetContext _datasetContext;
        private readonly IS3ServiceProvider _s3ServiceProvider;
        private readonly IAuthorizationProvider _authorizationProvider;
        private readonly IHttpClientGenerator _httpClientGenerator;
        #endregion

        #region Constructor
        public PagingHttpsJobProvider(IDatasetContext datasetContext, IS3ServiceProvider s3ServiceProvider, IAuthorizationProvider authorizationProvider, IHttpClientGenerator httpClientGenerator) 
        {
            _datasetContext = datasetContext;
            _s3ServiceProvider = s3ServiceProvider;
            _authorizationProvider = authorizationProvider;
            _httpClientGenerator = httpClientGenerator;
        }
        #endregion

        #region IBaseJobProvider Implementation
        public void Execute(RetrieverJob job)
        {
            using (_authorizationProvider)
            {
                HTTPSSource source = (HTTPSSource)job.DataSource;

                using (HttpClient httpClient = _httpClientGenerator.GenerateHttpClient(job.DataSource.BaseUri.ToString()))
                {
                    httpClient.BaseAddress = source.BaseUri;
                    httpClient.Timeout = new TimeSpan(0, 10, 0);

                    if (source.SourceAuthType.Is<OAuthAuthentication>())
                    {
                        foreach (DataSourceToken dataSourceToken in source.Tokens)
                        {
                            RetrieveDataAsync(job, httpClient, dataSourceToken).Wait();
                        }
                    }
                    else
                    {
                        RetrieveDataAsync(job, httpClient, null).Wait();
                    }
                }
            }
        }
        #endregion

        #region Private
        private async Task RetrieveDataAsync(RetrieverJob job, HttpClient httpClient, DataSourceToken dataSourceToken)
        {
            //get data flow step for drop location info
            HTTPSSource source = (HTTPSSource)job.DataSource;
            DataFlowStep s3DropStep = _datasetContext.DataFlowStep.FirstOrDefault(w => w.DataFlow.Id == job.DataFlow.Id && w.DataAction_Type_Id == DataActionType.ProducerS3Drop);
            string dataPath = job.FileSchema.SchemaRootPath;

            //build starting url
            string relativeUri = BuildRelativeUri(job);

            //build filename
            int pageNumber = 1;
            string filename = GetFileName(job, dataSourceToken);
            string tempFile = GetTempFile(job.Id, filename);

            bool hasMore = true;

            try
            {
                using (FileStream fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.ReadWrite))
                {
                    //loop until no more to retrieve
                    do
                    {
                        SetAuthorizationHeader(source, dataSourceToken, httpClient);

                        //make request
                        using (HttpResponseMessage response = await httpClient.GetAsync(relativeUri, HttpCompletionOption.ResponseHeadersRead))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                                using (StreamReader streamReader = new StreamReader(contentStream))
                                using (JsonReader jsonReader = new JsonTextReader(streamReader))
                                {
                                    JObject responseObj = JObject.Load(jsonReader);

                                    //get the count of data from response object to determine to continue
                                    if (responseObj.SelectToken(dataPath)?.Any() == true)
                                    {
                                        //combined size of file and response is over 2GB
                                        if (contentStream.Length + fileStream.Length > Math.Pow(1024, 3) * 2)
                                        {
                                            //upload to S3 and save progress
                                            UploadToS3(fileStream, s3DropStep, $"{filename}_{pageNumber}");

                                            //clear contents of filestream for continued use
                                            fileStream.SetLength(0);
                                            fileStream.Flush();
                                        }

                                        //if this doesn't write in a single line, we'll write unformatted JObject
                                        await contentStream.CopyToAsync(fileStream);

                                        //set next page
                                        switch (job.JobOptions.HttpOptions.PagingType)
                                        {
                                            case PagingType.PageNumber:
                                                //add the next page number to uri

                                                break;
                                        }
                                    }
                                    else
                                    {
                                        hasMore = false;
                                    }
                                }
                            }
                            else
                            {
                                string errorMessage = $"HTTPS request to {relativeUri} failed. {response.Content.ReadAsStringAsync().Result}";
                                Logger.Error(errorMessage);
                                throw new HttpsJobProviderException(errorMessage);
                            }
                        }

                    } while (hasMore);

                    //upload to S3 and save progress (last page retrieved had no results)
                    UploadToS3(fileStream, s3DropStep, $"{filename}_{pageNumber - 1}");

                    //clear execution parameters
                }
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        private string GetFileName(RetrieverJob job, DataSourceToken dataSourceToken)
        {
            string filename = job.JobOptions.TargetFileName;
            if (dataSourceToken != null)
            {
                filename += "_" + dataSourceToken.TokenName ?? dataSourceToken.Id.ToString();
            }

            return filename;
        }

        private void SetAuthorizationHeader(HTTPSSource source, DataSourceToken dataSourceToken, HttpClient httpClient)
        {
            if (source.SourceAuthType.Is<OAuthAuthentication>())
            {
                //make sure token is not expired
                string token = _authorizationProvider.GetOAuthAccessToken(source, dataSourceToken);
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            }
            else if (source.SourceAuthType.Is<TokenAuthentication>())
            {
                string token = _authorizationProvider.GetTokenAuthenticationToken(source);
                httpClient.DefaultRequestHeaders.Add(source.AuthenticationHeaderName, token);
            }
        }

        private string BuildRelativeUri(RetrieverJob job)
        {
            string relativeUri = job.RelativeUri;

            foreach (RequestVariable variable in job.RequestVariables)
            {
                string fullVariableName = string.Format(Indicators.REQUESTVARIABLEINDICATOR, variable.VariableName);
                string variableValue = GetVariableValue(variable);
                relativeUri = relativeUri.Replace(fullVariableName, variableValue);
            }

            return relativeUri;
        }

        private string GetVariableValue(RequestVariable variable)
        {
            if (variable.VariableIncrementType == RequestVariableIncrementType.Daily)
            {
                DateTime previousDate = DateTime.ParseExact(variable.VariableValue, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime nextDate = previousDate.AddDays(1);
                return nextDate.ToString("yyyy-MM-dd");
            }

            return "";
        }

        private string GetTempFile(int jobId, string filename)
        {
            string tempDirectory = Path.Combine(Configuration.Config.GetHostSetting("GoldenEyeWorkDir"), "Jobs", jobId.ToString());
            Directory.CreateDirectory(tempDirectory);

            return $@"{tempDirectory}\{filename}.json";
        }

        private void UploadToS3(FileStream fileStream, DataFlowStep s3DropStep, string filename)
        {
            _s3ServiceProvider.UploadDataFile(fileStream, s3DropStep.TriggerBucket, $"{s3DropStep.TriggerKey}{filename}.json");
            
            //save progress

        }

        private void SetNextPageRequest()
        {

        }

        private void AddUriParameter(ref string uri, string parameterKey, string parameterValue)
        {
            uri += (uri.Contains("?") ? "&" : "?") + $"{parameterKey}={Uri.EscapeDataString(parameterValue)}";
        }
        #endregion

        #region Not Implemented
        public void ConfigureProvider(RetrieverJob job)
        {
            throw new NotImplementedException();
        }

        public void Execute(RetrieverJob job, string filePath)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
