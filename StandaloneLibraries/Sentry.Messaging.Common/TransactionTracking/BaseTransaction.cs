using System;
using System.Runtime.InteropServices;
using Sentry.Common;

namespace Sentry.Messaging.Common
{
    public class BaseTransaction
    {
        public Guid Id { get; protected set; }
        public DateTime StoredDate { get; set; }
    }
}