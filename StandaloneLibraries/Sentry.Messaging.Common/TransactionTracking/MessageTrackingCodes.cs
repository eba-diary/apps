namespace Sentry.Messaging.Common
{
    public static class MessageTrackingFailureCodes
    {
        public const string FailureCodeValidation = "E01";
        public const string FailureCodeInvalidFormat = "E02";
        public const string FailureCodeUnknown = "E03";
        public const string FailureConnection = "E04";
        public const string FailureParse = "E05";
        public const string FailureIdentify = "E06";
        public const string FailureTransform = "E07";
        public const string FailureUnauthorized = "E08";
        public const string FailureWebService = "E09";
        public const string FailurePublish = "E10";
    }

    public static class MessageActionCodes
    {
        public const string MessageActionBegin = "BEGIN";
        public const string MessageActionSuccess = "SUCCESS";
        public const string MessageActionFailure = "FAILURE";
        public const string MessageActionSkip = "SKIP";
    }

    public static class SerializeMessageOptions
    {
        public const string AlwaysSerialize = "ALWAYS";
        public const string SerializeOriginalMessageOnly = "ORIGINALONLY";
        public const string SerializeEndActions = "ENDACTION";
    }
}
