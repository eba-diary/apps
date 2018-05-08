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
using Newtonsoft.Json;
using Ionic.Zip;

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
                var dscontext = container.GetInstance<IDatasetContext>();
                
                Logger.Debug($"DatasetRetriever Started with Request ID: {_requestId}");
                RTRequest request = requestcontext.GetRequest(_requestId);

                if (request.SourceType.Type == "FTP")
                {
                    ProcessFTPRequest(request, ftpprovider, dscontext);
                }
                else if (request.SourceType.Type == "API")
                {
                    ProcessWeatherRequest_v2(weatherService, request);
                }
                else if(request.SourceType.Type == "HTTPS")
                {
                    processHTTPRequest(dscontext, request);
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

        private static void ProcessFTPRequest(RTRequest request, IFtpProvider ftpprovider, IDatasetContext dscontext)
        {
            RequestOptions reqOptions = JsonConvert.DeserializeObject<RequestOptions>(request.Options);


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

            url = $@"{request.SourceType.BaseUrl}\{request.Endpoint.Value}\{filename}";

            //If incoming file is not compressed (or business wants the compressed formatted file to be uploaded), stream to location specified on datasetfileconfig
            if (!reqOptions.IsCompressed)
            {
                try
                {
                    throw new NotImplementedException();
                    //DatasetFileConfig TargetConfig = dscontext.GetById<DatasetFileConfig>(reqOptions.DataConfigId);
                    //ftpprovider.DownloadFile(url.ToString(), new NetworkCredential("anonymous", "Jered.Gosse@Sentry.com"), TargetConfig.DropPath + filename);
                }
                catch (Exception e)
                {
                    Logger.Error("Error Retrieving FTP File", e);
                }
            }
            //Incoming file is compressed, therefore, interrogate reqOptions to determine what to do with compressed file
            else
            {
                throw new NotImplementedException();
                ////Pull incoming file to storage local to application
                //ftpprovider.DownloadFile(url.ToString(), new NetworkCredential("anonymous", "Jered.Gosse@Sentry.com"), $"C:\\tmp\\DatasetRetriever\\{request.Id}");

                //ProcessCompressedFile(dscontext, request, reqOptions);
            }
            
        }

        private static void processHTTPRequest(IDatasetContext dscontext, RTRequest request)
        {

            //RequestOptions options = new RequestOptions();
            //options.IsCompressed = true;
            //options.CompressionFormat = CompressionTypes.ZIP;
            //options.CompressedFileRules = new List<CompressedFileRule>();
            //options.CompressedFileRules.Add(new CompressedFileRule("ReadMe_Census.txt", 232, false));
            //options.CompressedFileRules.Add(new CompressedFileRule(@"\basdfasf(?:Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec?)(?:19[7-9]\d|2\d{3}).txt", 228, true));

            //string jsonRequest = JsonConvert.SerializeObject(options, Formatting.Indented);

            //using (MemoryStream ms = new MemoryStream())
            //{
            //    StreamWriter writer = new StreamWriter(ms);

            //    writer.WriteLine(jsonRequest);
            //    writer.Flush();

            //    //You have to rewind the MemoryStream before copying
            //    ms.Seek(0, SeekOrigin.Begin);

            //    using (FileStream fs = new FileStream($"C:\\tmp\\OptionsObject.json", FileMode.OpenOrCreate))
            //    {
            //        ms.CopyTo(fs);
            //        fs.Flush();
            //    }
            //}

            RequestOptions reqOptions = JsonConvert.DeserializeObject<RequestOptions>(request.Options);

            try
            {
                WebRequest webReq = WebRequest.Create(request.SourceType.BaseUrl);
                WebResponse response = webReq.GetResponse();


                //If incoming file is not compressed (or business wants the compressed formatted file to be uploaded), stream to location specified on datasetfileconfig
                if (!reqOptions.IsCompressed)
                {
                    DatasetFileConfig TargetConfig = dscontext.GetById<DatasetFileConfig>(reqOptions.DataConfigId);

                    using (FileStream fs = new FileStream(TargetConfig.DropPath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        using (Stream dataStream = response.GetResponseStream())
                        {
                            dataStream.CopyTo(fs);
                            fs.Flush();
                            dataStream.Close();
                        }
                        fs.Close();
                    }
                }

                //Incoming file is compressed, therefore, interrogate reqOptions to determine what to do with compressed file
                else
                {
                    //Pull incoming file to storage local to application
                    using (FileStream fs = new FileStream($"C:\\tmp\\DatasetRetriever\\{request.Id}", FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        using (Stream dataStream = response.GetResponseStream())
                        {
                            dataStream.CopyTo(fs);
                            fs.Flush();
                            dataStream.Close();
                        }
                        fs.Close();
                    }

                    ProcessCompressedFile(dscontext, request, reqOptions);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error Processing Request", ex);
            }
        }

        private static void ProcessCompressedFile(IDatasetContext dscontext, RTRequest request, RequestOptions reqOptions)
        {
            //Determine appropriate compression processing
            if (reqOptions.CompressionFormat == CompressionTypes.ZIP)
            {

                //Current Assumpiton, if ZIP contains multiple files they are all streamed to the same DropPath
                //Potential Future expansion: If multiple files exist, each could potentially be extracted to a separate DropPath

                //Determine if there are rules to apply to the files within the compressed file
                if (reqOptions.CompressedFileRules != null)
                {
                    using (ZipFile zip = ZipFile.Read($"C:\\tmp\\DatasetRetriever\\{request.Id}"))
                    {
                        //Loop through each file within .ZIP file
                        foreach (ZipEntry e in zip)
                        {
                            foreach (CompressedFileRule rule in reqOptions.CompressedFileRules)
                            {
                                if (!rule.IsRegexSearch)
                                {
                                    if (e.FileName == rule.FileSearch)
                                    {
                                        DatasetFileConfig dfc = dscontext.GetById<DatasetFileConfig>(rule.DataConfigId);

                                        using (FileStream fs = new FileStream((dfc.DropPath + e.FileName), FileMode.OpenOrCreate, FileAccess.ReadWrite))
                                        {
                                            e.Extract(fs);
                                            fs.Flush();
                                            fs.Close();
                                        }
                                        //only process file to first rule match
                                        break;
                                    }
                                }
                                else if (rule.IsRegexSearch)
                                {
                                    if (Regex.IsMatch(e.FileName, rule.FileSearch))
                                    {
                                        DatasetFileConfig dfc = dscontext.GetById<DatasetFileConfig>(rule.DataConfigId);

                                        using (FileStream fs = new FileStream((dfc.DropPath + e.FileName), FileMode.OpenOrCreate, FileAccess.ReadWrite))
                                        {
                                            e.Extract(fs);
                                            fs.Flush();
                                            fs.Close();
                                        }
                                    }
                                    //only process file to first rule match
                                    break;
                                }
                            }
                        }
                    }
                }
                //No rules exist, therefore, file(s) within compressed file will be extracted to DropPath attached to DatasetFileConfig on request
                else
                {
                    DatasetFileConfig TargetConfig = dscontext.GetById<DatasetFileConfig>(reqOptions.DataConfigId);

                    //Stream all files within ZIP file to DropPath attached to TargetConfig
                    using (ZipFile zip = ZipFile.Read($"C:\\tmp\\DatasetRetriever\\{request.Id}"))
                    {
                        foreach (ZipEntry e in zip)
                        {
                            using (FileStream fs = new FileStream((TargetConfig.DropPath + e.FileName), FileMode.OpenOrCreate, FileAccess.ReadWrite))
                            {
                                e.Extract(fs);
                                fs.Flush();
                                fs.Close();
                            }
                        }
                    }
                }


                //If multiple files stream each indiviual file to DropLocation for DatasetFileConfig attached to request


                //If single file, stream to file to DropLocation for DatasetFileConfig attached to request

                //Cleanup work file
                Logger.Info("Removing processed request file");
                System.IO.File.Delete($"C:\\tmp\\DatasetRetriever\\{request.Id}");
            }
            else
            {
                Logger.Error($"Specified Unsupported Compression Type: {reqOptions.CompressionFormat}");
                throw new Exception($"Unsupported Compression Format: {reqOptions.CompressionFormat}");
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
