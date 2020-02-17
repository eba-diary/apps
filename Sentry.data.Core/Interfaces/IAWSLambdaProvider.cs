using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IAWSLambdaProvider
    {
        void ConfigureClient(LambdaClientConfig config);
        void SetInvocationType(string invocationType);
        void SetLogType(string logType);
        void SetFunctionName(string functionName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload">The data to pass to the lambda function</param>
        /// <exception cref="AWSLambdaInvalidParameterException">Thrown when parameter is invalid</exception>
        /// <exception cref="AWSLambdaNotFoundException">Thrown when lambda function is not found</exception>
        /// <exception cref="AWSLambdaServiceException">Thrown when for lambda service level exceptions</exception>
        /// <exception cref="AWSLambdaRequestLimitException">Thrown when request limit has been excceeded</exception>
        /// <exception cref="AWSLambdaException">Thrown for unhandled exceptions</exception>
        void InvokeFunction(string payload);
    }
}
