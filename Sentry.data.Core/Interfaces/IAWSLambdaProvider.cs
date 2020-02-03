using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IAWSLambdaProvider
    {
        void ConfigureClient(string awsRegion, string accessKey, string secretKey);
        void SetInvocationType(string invocationType);
        void SetLogType(string logType);
        void SetFunctionName(string functionName);
        void InvokeFunction(string payload);
    }
}
