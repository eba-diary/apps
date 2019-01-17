namespace Sentry.data.Core
{
    public static class GlobalConstants
    {


        public static class ValidationErrors
        {
            public const string S3KEY_IS_BLANK = "keyIsBlank";
            public const string CATEGORY_IS_BLANK = "categoryIsBlank";
            public const string NAME_IS_BLANK = "nameIsBlank";
            public const string CREATION_USER_NAME_IS_BLANK = "creationUserNameIsBlank";
            public const string UPLOAD_USER_NAME_IS_BLANK = "uploadUserNameIsBlank";
            public const string DATASET_DATE_IS_OLD = "datasetDateIsOld";
            public const string DATASET_DESC_IS_BLANK = "descIsBlank";
            public const string SENTRY_OWNER_IS_NOT_NUMERIC = "sentryOwnerIsNotNumeric";
            public const string NUMBER_OF_FILES_IS_NEGATIVE = "numberOfFilesIsNegative";

            //Report specific validation errors
            public const string LOCATION_IS_BLANK = "locationIsBlank";
            public const string LOCATION_IS_INVALID = "locationIsInvalid";
        }


        public static class DataEntityTypes
        {
            public const string REPORT = "RPT";
            public const string DATASET = "DTS"; //currently not being used becuase datasets have a type = null
        }


    }
}
