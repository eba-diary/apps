using System;
using System.Collections.Generic;
using Sentry.Common.Logging;
using Sentry.Configuration;
using System.Linq;
using System.Text;
using Sentry.data.Infrastructure;
using StructureMap;
using Amazon.Lambda;
using Amazon;
using Amazon.Lambda.Model;
using Sentry.data.Core;
using Sentry.data.Core.Entities.S3;
using Newtonsoft.Json;

namespace TESTING_CONSOLE
{
    public class Class1
    {
        private static IAmazonLambda _lambdaClient = null;

        static void Main(string[] args)
        {
            Logger.LoggingFrameworkAdapter = new Sentry.Common.Logging.Adapters.Log4netAdapter(Config.GetHostSetting("AppLogger"));

            Logger.Info("Starting TESTING_CONSOLE");
            //Call your bootstrapper to initialize your application
            Bootstrapper.Init();

            //create an IOC (structuremap) container to wrap this transaction
            using (IContainer container = Bootstrapper.Container.GetNestedContainer())
            {
                //    var service = container.GetInstance<MyService>();
                //    var result = service.DoWork();
                //    container.GetInstance<ITESTING_CONSOLEContext>.SaveChanges();
                AmazonLambdaConfig ldConfig = new AmazonLambdaConfig();
                ldConfig.RegionEndpoint = RegionEndpoint.GetBySystemName(Config.GetSetting("AWSRegion"));
                //if (!String.IsNullOrWhiteSpace(Config.GetHostSetting("SentryS3ProxyHost")))
                //{
                //    ldConfig.ProxyHost = Config.GetHostSetting("SentryS3ProxyHost");
                //    ldConfig.ProxyPort = int.Parse(Config.GetSetting("SentryS3ProxyPort"));
                //}
                ldConfig.ProxyCredentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                string awsAccessKey = Config.GetHostSetting("AWSAccessKey");
                string awsSecretKey = Config.GetHostSetting("AWSSecretKey");

                _lambdaClient = new AmazonLambdaClient(awsAccessKey, awsSecretKey, ldConfig);


                GetFunctionRequest req = new GetFunctionRequest()
                {
                    FunctionName = "dataset-preview-np"
                };

                ////Mock for testing... sent mock s3object created 
                //S3LamdaEvent s3e = new S3LamdaEvent()
                //{
                //    Records = new List<S3ObjectEvent>()
                //    {
                //        new S3ObjectEvent()
                //        {
                //            eventName = "ObjectCreated:Put",
                //            s3 = new S3()
                //            {
                //                bucket = new Bucket()
                //                {
                //                    name = "sentry-dataset-management-np-nr"
                //                },
                //                _object = new Sentry.data.Core.Entities.S3.Object()
                //                {
                //                    key = $"data-dev/0020001/2018/5/24/20180524_area_titles_A19E0536EC6.csv"
                //                }
                //            }
                //        }
                //    }
                //};

                //Mock for testing... sent mock s3object created 
                S3LamdaEvent s3e = new S3LamdaEvent()
                {
                    Records = new List<S3ObjectEvent>()
                    {
                        new S3ObjectEvent()
                        {
                            eventName = "ObjectCreated:Put",
                            s3 = new S3()
                            {
                                bucket = new Bucket()
                                {
                                    name = "sentry-dataset-management-np"
                                },
                                _object = new Sentry.data.Core.Entities.S3.Object()
                                {
                                    key = $"data-dev/sentry/customeronelinking/184/2017/7/27/CustomerOneAccountLinking.csv"
                                }
                            }
                        }
                    }
                };

                InvokeRequest invokeReq = new InvokeRequest()
                {
                    FunctionName = "dataset-preview-np",
                    InvocationType = "RequestResponse",
                    LogType = "Tail",
                    Payload = JsonConvert.SerializeObject(s3e)
                };

                try
                {
                    //GetFunctionResponse resp = _lambdaClient.GetFunction(req);




                    InvokeResponse resp = _lambdaClient.Invoke(invokeReq);
                    var logresult = Encoding.UTF8.GetString(Convert.FromBase64String(resp.LogResult));
                    var payload = Encoding.UTF8.GetString(resp.Payload.ToArray());
                    Console.WriteLine(logresult);
                    Console.WriteLine(payload);

                    Logger.Info($"Lambda Log Result: {logresult}");
                    Logger.Info($"Lambda Payload: {payload}");


                }
                catch (InvalidParameterValueException paramEx)
                {
                    Logger.Error("One of the parameters in the request is invalid.", paramEx);
                }
                catch (ResourceNotFoundException resourceEx)
                {
                    Logger.Error("The resource specified in the request does not exist.", resourceEx);
                }
                catch (ServiceException serviceEx)
                {
                    Logger.Error("The AWS Lambda service encountered an internal error.", serviceEx);
                }
                catch (TooManyRequestsException requestLimitEx)
                {
                    Logger.Error("The request throughput limit was exceeded.", requestLimitEx);
                }
                catch (Exception ex)
                {
                    Logger.Error("The request throughput limit was exceeded.", ex);
                }
                


                //provider.ExecuteDependenciesAsync("sentry-dataset-management-np-nr", "data/17/TestFile.csv");
            }

            Logger.Info("Console App completed successfully.");
        }
    }
}
