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
            public const string VERSION_DATAFLOW = VERSION + "/dataflow";
            public const string VERSION_TAG = VERSION + "/tags";
            public const string VERSION_DATAFILE = VERSION + "/datafile";
        }

        public static class FilterCategoryDisplayNames
        {
            //Not sure if this is how I want to make it consistant across the code
            public const string ENVIRONMENT = "Environment";
            public const string SENSITIVITY = "Sensitivity";
        }
    }
}