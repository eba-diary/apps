using System;
using System.Collections.Generic;
using Sentry.Common.Logging;
using Sentry.Configuration;
using System.Linq;
using System.Text;
using RestSharp;
using Newtonsoft;
using Newtonsoft.Json.Linq;
using StructureMap;
using Sentry.data.Infrastructure;
using Sentry.data.Core;
using System.IO;
using Sentry.Core;
using System.Net;
using System.Text.RegularExpressions;

namespace Sentry.data.DatasetRetriever
{
    /// <summary>
    /// 
    /// </summary>
    public class Class1
    {
        /// <summary>
        /// 
        /// </summary>
        //public static IContainer container;
        /// <summary>
        /// 
        /// </summary>
        public static int _requestId;        

        static int Main(string[] args)
        {

            log4net.GlobalContext.Properties["RequestID"] = _requestId;
            Logger.LoggingFrameworkAdapter = new Sentry.Common.Logging.Adapters.Log4netAdapter(Config.GetHostSetting("AppLogger"));

            try
            {
                CheckArguments(args);
            }
            catch (ArgumentException)
            {
                return (int)ExitCodes.ArgumentError;
            }

            //Call your bootstrapper to initialize your application
            Sentry.data.Infrastructure.Bootstrapper.Init();


            //create an IOC (structuremap) container to wrap this transaction
            using (IContainer container = Sentry.data.Infrastructure.Bootstrapper.Container.GetNestedContainer())
            {
                var weatherService = container.GetInstance<IWeatherDataProvider>();
                var requestcontext = container.GetInstance<IRequestContext>();
                var ftpprovider = container.GetInstance<IFtpProvider>();
                
                Logger.Debug($"DatasetRetriever Started with Request ID: {_requestId}");
                RTRequest request = requestcontext.GetRequest(_requestId);

                if (request.SourceType.Type == "FTP")
                {
                    ProcessFTPRequest(request, ftpprovider);
                }
                else if (request.SourceType.Type == "API")
                {
                    ProcessWeatherRequest_v2(weatherService, request);
                }
                
            }
            
            Logger.Info($"Console App completed successfully for RequestID:.{_requestId}");

            return (int)ExitCodes.Success;
        }

        private static IRestResponse ProcessAPIRequest(IWeatherDataProvider weatherAPIProvider, RTRequest request, out RestClient _client, out RestRequest _request, IRestResponse _response)
        {
            _client = new RestClient();
            _client.BaseUrl = new Uri(request.SourceType.BaseUrl);
            _client.Proxy = new WebProxy(Configuration.Config.GetHostSetting("SentryWebProxyHost"));
            _client.Proxy.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
            _request = new RestRequest();

            _request.AddHeader("token", "SJCvXfeARwTJNbbZDiLHSPcjSyeNUtwf");

            _request.AddParameter("endpoint", request.Endpoint.Name, ParameterType.UrlSegment);

            _request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };

            foreach (var parameter in request.Parameters)
            {
                _request.AddQueryParameter(parameter.ApiParameter.Name, parameter.Value);
            }

            Console.WriteLine(_client.BuildUri(_request));
            try
            {

                _response = _client.Execute(_request);

                if (_response.ErrorException != null)
                {
                    const string message = "Error retieving response. Check inner details for more info.";
                    var e = new ApplicationException(message, _response.ErrorException);
                    throw e;
                }

            }
            catch (Exception e)
            {
                Logger.Error("Error retreiving Weather data", e);
            }

            return _response;
        }

        private static void ProcessWeatherRequest_v2(IWeatherDataProvider weatherAPIProvider, RTRequest request)
        {
            RestRequest req = new RestRequest();
            IRestResponse response = null;
            string filename = null;
            StringBuilder destination = new StringBuilder();

            RestClient client = weatherAPIProvider.CreateClient(request.SourceType.BaseUrl);

            req.AddHeader("token", Configuration.Config.GetHostSetting($"{request.SourceType.Name}_APIKey"));

            req.AddParameter("endpoint", request.Endpoint.Name, ParameterType.UrlSegment);

            req.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };

            req.Timeout = 180000;
            Logger.Info("API Request Timeout: " + req.Timeout.ToString());

            foreach (var parameter in request.Parameters)
            {
                if (parameter.ApiParameter.Name == "DestLocation")
                {
                    destination.Append(Configuration.Config.GetHostSetting("DatasetLoaderDFSPath"));
                    destination.Append(@"\");
                    destination.Append(parameter.Value);
                }
                else if (parameter.ApiParameter.Name == "DestFileName")
                {
                    filename = parameter.Value;
                }
                else
                {
                    req.AddQueryParameter(parameter.ApiParameter.Name, parameter.Value);
                }
            }

            destination.Append($@"\{filename}");

            DateTime reqStartTime = DateTime.Now;
            TimeSpan reqTotalTime = TimeSpan.MinValue;
            Logger.Info(client.BuildUri(req).ToString());
            try
            {
                response = client.Execute(req);

                if (response.ErrorException != null)
                {
                    Logger.Info("Total Request Time - failed Request (milsec): " + (DateTime.Now - reqStartTime).TotalMilliseconds.ToString());
                    const string message = "Error retieving response. Check inner details for more info.";
                    var e = new ApplicationException(message, response.ErrorException);
                    throw e;
                }
                else
                {
                    Logger.Info("Total Request Time (milsec): " + (DateTime.Now - reqStartTime).TotalMilliseconds.ToString());
                }

            }
            catch (Exception e)
            {
                Logger.Error("Error retreiving Weather data", e);
                Environment.Exit((int)ExitCodes.Failure);
            }

            if (response.Content != null)
            {
                Logger.Debug("Reponse Content (first 300 characters):" + response.Content);
            }
            else
            {
                Logger.Debug("Response Content is Null");
            }            

            JObject jobject = JObject.Parse(response.Content);


            File.WriteAllText(destination.ToString(), jobject.ToString());

        }

        private static void ProcessFTPRequest(RTRequest request, IFtpProvider ftpprovider)
        {
            string filename = null;
            StringBuilder destination = new StringBuilder();
            string url = null;           

            foreach (var param in request.Parameters)
            {
                if (param.ApiParameter.Name == "Destination")
                {
                    destination.Append(Configuration.Config.GetHostSetting("DatasetLoaderDFSPath"));
                    destination.Append(@"\");
                    destination.Append(param.Value);
                }

                if (param.ApiParameter.Name == "Filename")
                {
                    filename = param.Value;
                }
                                               
            }

            destination.Append($@"\{filename}");

            url = $@"{request.SourceType.BaseUrl}\{request.Endpoint.Value}\{filename}";

            Logger.Info("url: " + url);
            Logger.Info("Destination: " + destination.ToString());

            try
            {
                ftpprovider.DownloadFile(url.ToString(), new NetworkCredential("anonymous", "Jered.Gosse@Sentry.com"), destination.ToString());
            }
            catch (Exception e)
            {
                Logger.Error("Error Retrieving FTP File", e);
            }
            
        }

        private static void CheckArguments(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-RequestId":
                        i++;
                        if (args.Length <= i) throw new ArgumentException(args[i]);

                        //ensure -d value is only numbers
                        if (Regex.IsMatch(args[i], "^[0-9]*$"))
                        {
                            _requestId = Int32.Parse((args[i]));
                            break;
                        }
                        else
                        {
                            Logger.Error($"Reqeust ID Not an Integer: {args[i]}");
                            throw new ArgumentException(args[i]);
                        }                        

                    default:
                        throw new ArgumentException("Invalid Arguments");
                }
            }
        }
    }
}
