using System;
using System.Runtime.InteropServices;
using Sentry.Common;

namespace Sentry.Messaging.Common
{
    public class BaseEventMessage
    {
        public string EventType { get; set; }
    }
}