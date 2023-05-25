using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Core.Exceptions;
using System;
using System.Text;

namespace Sentry.data.Infrastructure
{
    public class AWSLambdaProvider : IAWSLambdaProvider, IDisposable
    {
        private IAmazonLambda _lambdaClient;
        private string _invocationType;
        private string _logType;
        private string _functionName;

        public AWSLambdaProvider() { }

        public void ConfigureClient(LambdaClientConfig config)
        {
            AmazonLambdaConfig ldConfig = new AmazonLambdaConfig();
            ldConfig.RegionEndpoint = RegionEndpoint.GetBySystemName(config.AWSRegion);
            //proxy only needed when not running on AWS.  Calling code expected to pass empty value if proxy host not needed.
            if (!String.IsNullOrWhiteSpace(config.ProxyHost))
            {
                ldConfig.ProxyHost = config.ProxyHost;
                ldConfig.ProxyPort = int.Parse(config.ProxyPort);
            }
            ldConfig.ProxyCredentials = System.Net.CredentialCache.DefaultCredentials;

            _lambdaClient = new AmazonLambdaClient(config.AccessKey, config.SecretKey, ldConfig);
        }

        public void SetInvocationType(string invocationType)
        {
            _invocationType = invocationType;
        }

        public void SetLogType(string logType)
        {
            _logType = logType;
        }

        public void SetFunctionName(string functionName)
        {
            _functionName = functionName;
        }

        public void InvokeFunction(string payload)
        {
            InvokeRequest req = new InvokeRequest()
            {
                FunctionName = _functionName,
                InvocationType = _invocationType,
                LogType = _logType,
                Payload = payload
            };

            try
            {
                InvokeResponse resp = _lambdaClient.Invoke(req);
                Logger.Debug($"awslambdaprovider-invokefunction lambdafunction:{req.FunctionName} logresult:{Encoding.UTF8.GetString(Convert.FromBase64String(resp.LogResult))} payloadresult:{Encoding.UTF8.GetString(resp.Payload.ToArray())}");
            }
            catch (InvalidParameterValueException paramEx)
            {
                throw new AwsLambdaInvalidParameterException("One of the parameters in the request is invalid.", paramEx);
            }
            catch (Amazon.Lambda.Model.ResourceNotFoundException resourceEx)
            {
                throw new AwsLambdaNotFoundException("The resource specified in the request does not exist.", resourceEx);
            }
            catch (ServiceException serviceEx)
            {
                throw new AwsLambdaServiceException("The AWS Lambda service encountered an internal error.", serviceEx);
            }
            catch (TooManyRequestsException requestLimitEx)
            {
                throw new AwsLambdaRequestLimitException("The request throughput limit was exceeded.", requestLimitEx);
            }
            catch (Exception ex)
            {
                throw new AwsLambdaException("Error Unknown", ex);
            }
        }

        public void Dispose()
        {
            if(_lambdaClient != null)
            {
                _lambdaClient.Dispose();
            }
        }
    }
}
