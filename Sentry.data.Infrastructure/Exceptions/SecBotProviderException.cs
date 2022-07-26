using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.Exceptions
{
    /// <summary>
    /// This Exception is thrown when there are errors communicating with the SecBot API
    /// </summary>
    [Serializable]
    public class SecBotProviderException : Exception
    {
        public SecBotProviderException() { }
        public SecBotProviderException(string message) : base(message) { }
        public SecBotProviderException(string message, Exception inner) : base(message, inner) { }
        protected SecBotProviderException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
