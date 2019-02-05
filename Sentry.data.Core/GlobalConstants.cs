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
            public const string DATASET = "DS"; 
        }

        public static class EventType
        {
            public const string VIEWED = "Viewed";
            public const string DOWNLOAD = "Downloaded Data File";
            public const string EDITED_DATA_FILE = "Edited Data File";

            public const string VIEWED_REPORT = "Viewed Report";
            public const string CREATED_REPORT = "Created Report";
            public const string UPDATED_REPORT = "Updated Report";

            public const string VIEWED_DATASET = "Viewed Dataset";
            public const string CREATED_DATASET = "Created Dataset";
            public const string UPDATED_DATASET = "Updated Dataset";
        }

        public static class Statuses
        {
            public const string SUCCESS = "Success";
        }

        public static class DataElementCode
        {
            public const string DATA_FILE = "f";
        }

        public static class DataElementDescription
        {
            public const string BUSINESS_TERM = "Business Term";
            public const string LINEAGE = "Lineage";
            public const string DATA_FILE = "Data File";
        }

        public static class DataSourceName
        {
            public const string DEFAULT_DROP_LOCATION = "Default Drop Location";
            public const string DEFAULT_S3_DROP_LOCATION = "Default S3 Drop Location";
        }

        public static class DataFeedType
        {
            public const string SAS = "SAS";
            public const string Tab = "TAB";
            public const string Datasets = "Datasets";
            public const string DataAssets = "Data Assets";
            public const string Exhibits = "Exhibits";
        }

        public static class BusinessObjectExhibit
        {
            public const string GET_LATEST_URL_PARAMETER = "&sInstance=Last";
        }

    }
}
