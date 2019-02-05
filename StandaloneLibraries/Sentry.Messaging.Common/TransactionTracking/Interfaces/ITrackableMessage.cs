using Sentry.AsyncCommandProcessor;
using System;
using System.Collections.Generic;

namespace Sentry.Messaging.Common
{
    public interface ITrackableMessage : ICommand
    {
        string GetOriginalMessageId();
        string GetMessageId();
        string GetMessageType();
        string GetMessageArea();
        string GetMessageSystemSource();
        string GetMessageDeliverySource();
        string GetMessageDetailedType();
        IEnumerable<string> GetMessageStepIds();
        DateTime GetMessageDate();
        string GetSerializedMessage();
        IDictionary<string, string> GetIdentifyingAttributes();
    }
}
