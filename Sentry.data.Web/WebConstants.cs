﻿namespace Sentry.data.Web
{
    public static class WebConstants
    {
        public static class Routes
        {
            public const string VERSION = "api/" + Sentry.WebAPI.Versioning.Constants.VERSION;

            public const string VERSION_APPLICATIONS = VERSION + "/applications";
            public const string VERSION_JOB = VERSION + "/jobs";
            public const string VERSION_ASSET = VERSION + "/assets";
            public const string VERSION_LINEAGE = VERSION + "/lineage";
            public const string VERSION_METADATA = VERSION + "/metadata";
            public const string VERSION_QUERYTOOL = VERSION + "/queryTool";
            public const string VERSION_TAG = VERSION + "/tags";
        }


    }
}