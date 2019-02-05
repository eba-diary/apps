using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.Messaging.Common
{
   public static class Util
    {
        [DllImport("rpcrt4.dll", SetLastError = true)]
        static extern int UuidCreateSequential(out Guid guid);

        public static Guid SequentialGuid()
        {
            const int RPC_S_OK = 0;
            Guid g;
            if (UuidCreateSequential(out g) != RPC_S_OK)
                return Guid.NewGuid();
            else
                return g;
        }
    }
}
