﻿namespace Sentry.data.Core
{
    public static class GlobalConstants
    {
        
        public static class System
        {
            public const string NAME = "Data.Sentry.Com";
            public const string ABBREVIATED_NAME = "DSC";
            public const string PROD = "PROD";
            public const string REQUEST_CONTEXT_GUID_FORMAT = "yyyyMMddHHmmssfff";
        }

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
            public const string SAID_ASSET_KEY_IS_BLANK = "SAIDAssetIsBlank";
        }


        public static class DataEntityCodes
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
            public const string DELETED_REPORT = "Deleted Report";
            public const string DOWNLOADED_REPORT = "Downloaded Report";

            public const string VIEWED_DATASET = "Viewed Dataset";
            public const string CREATED_DATASET = "Created Dataset";
            public const string UPDATED_DATASET = "Updated Dataset";
            public const string DELETE_DATASET = "Deleted Dataset";

            public const string DELETE_DATASET_SCHEMA = "Deleted Dataset Schema";
            public const string SYNC_DATASET_SCHEMA = "Sync Schema";

            public const string CREATED_TAG = "Created Tag";
            public const string UPDATED_TAG = "Updated Tag";

            public const string CREATED_DATASOURCE = "Created Data Source";
            public const string UPDATED_DATASOURCE = "Updated Data Source";

            public const string NOTIFICATION_CRITICAL = "Critical Notification";
            public const string NOTIFICATION_WARNING = "Warning Notification";
            public const string NOTIFICATION_INFO = "Info Notification";

            public const string NOTIFICATION_CRITICAL_ADD = "Critical Notification Add";
            public const string NOTIFICATION_WARNING_ADD = "Warning Notification Add";
            public const string NOTIFICATION_INFO_ADD = "Info Notification Add";

            public const string NOTIFICATION_CRITICAL_UPDATE = "Critical Notification Update";
            public const string NOTIFICATION_WARNING_UPDATE = "Warning Notification Update";
            public const string NOTIFICATION_INFO_UPDATE = "Info Notification Update";

            public const string DALE_SEARCH = "DaleQuery";

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
            public const string DEFAULT_HSZ_DROP_LOCATION = "Default HSZ Drop Location";
            public const string DEFAULT_DATAFLOW_DFS_DROP_LOCATION = "Default DataFlow DFS Drop Location";
        }

        public static class DataSoureDiscriminator
        {
            public const string DFS_SOURCE = "DFS";
            public const string DEFAULT_DROP_LOCATION = "DFSBasic";
            public const string DFS_CUSTOM = "DFSCustom";
            public const string DEFAULT_S3_DROP_LOCATION = "S3Basic";
            public const string FTP_SOURCE = "FTP";
            public const string S3_SOURCE = "S3";
            public const string SFTP_SOURCE = "SFTP";
            public const string HTTPS_SOURCE = "HTTPS";
            public const string JAVA_APP_SOURCE = "JavaApp";
            public const string GOOGLE_API_SOURCE = "GOOGLEAPI";
            public const string DEFAULT_HSZ_DROP_LOCATION = "DFSBasicHsz";

            public const string DEFAULT_DATAFLOW_DFS_DROP_LOCATION = "DFSDataFlowBasic";
            public const string FTP_DATAFLOW_SOURCE = "FTPDATAFLOW";
            public const string GOOGLE_API_DATAFLOW_SOURCE = "GOOGLEAPIDATAFLOW";
            public const string GENERIC_HTTPS_DATAFLOW_SOURCE = "GENERICHTTPSDATAFLOW";
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

        public static class HpsmTicketStatus
        {
            //DSC
            public const string PENDING = "Pending";
            public const string COMPLETED = "Completed";
            public const string DENIED = "Denied";
            public const string WIDHTDRAWN = "Withdrawn";
            //hpsm
            public const string APPROVED = "approved";
            public const string CLOSED = "closed";
            public const string IMPLEMENTATION = "SI:STD:Implementation";
            public const string LOG_AND_PREP = "SI:STD:Log and Prep";
        }

        public static class SecurableEntityName
        {
            public const string DATASET = "Dataset";
            public const string DATA_ASSET = "DataAsset";
            public const string DATASOURCE = "DataSource";
            public const string BUSINESSAREA = "BusinessArea";
        }

        public static class PermissionCodes
        {
            public const string CAN_PREVIEW_DATASET = "CanPreviewDataset";
            public const string CAN_VIEW_FULL_DATASET = "CanViewFullDataset";
            public const string CAN_QUERY_DATASET = "CanQueryDataset";
            public const string CAN_CONNECT_TO_DATASET = "CanConnectToDataset";
            public const string CAN_UPLOAD_TO_DATASET = "CanUploadToDataset";
            public const string CAN_MODIFY_NOTIFICATIONS = "CanModifyNotification";
            public const string CAN_USE_DATA_SOURCE = "CanUseDataSource";
            public const string CAN_MANAGE_SCHEMA = "CanManageSchema";

            public const string USE_APP = "UseApp";

            public const string DATASET_VIEW = "DatasetView";
            public const string DATA_ASSET_VIEW = "DataAssetView";
            public const string REPORT_VIEW = "ReportView";

            public const string DATASET_MODIFY = "DatasetModify";
            public const string DATA_ASSET_MODIFY = "DataAssetModify";
            public const string REPORT_MODIFY = "ReportModify";

            public const string USER_SWITCH = "UserSwitch";
            public const string ADMIN_USER = "AdminUser";

            public const string DALE_SENSITIVE_VIEW = "DaleSensView";
            public const string DALE_VIEW = "DaleView";
            public const string DALE_SENSITIVE_EDIT = "DaleSensEdit";
            public const string DALE_OWNER_VERIFIED_EDIT = "DaleOwnerVerifiedEdit";
        }

        public static class ConvertedFileStoragePrefix
        {
            public const string PARQUET_STORAGE_PREFIX = "parquet";
        }

        public static class SearchType
        {
            public const string BUSINESS_INTELLIGENCE_SEARCH = "BusinessIntelligence";
            public const string DATASET_SEARCH = "Datasets";
        }

        public static class StoragePrefixes
        {
            public const string DATASET_IMAGE_STORAGE_PREFIX = "images";
        }

        public static class Notifications
        {
            public const string DATAASSET_TYPE = "DA";
            public const string BUSINESSAREA_TYPE = "BA";
            public const int TITLE_MAX_SIZE = 250;
            public const int MESSAGE_MAX_SIZE = 1073741823;
        }

        public static class JobStates
        {
            public const string RETRIEVERJOB_STARTED_STATE = "Started";
            public const string RETRIEVERJOB_RUNNING_STATE = "Running";
            public const string RETRIEVERJOB_SUCCESS_STATE = "Success";
            public const string RETRIEVERJOB_FAILED_STATE = "Failed";
        }

        public static class CherwellBusinessObjectNames
        {
            public const string CHANGE_REQUEST = "ChangeRequest";
            public const string APPROVAL = "Approval";
            public const string USER = "UserInfo";
            public const string CUSTOMER = "CustomerInternal";
        }

        public static class CherwellChangeStatusNames
        {
            public const string LOGGING_AND_PREP = "Logging and Prep";
            public const string APPROVAL = "Approval";
            public const string IMPLEMENTING = "Implementing";
            public const string CLOSED = "Closed";
        }

        public static class CherwellChangeStatusOrder
        {
            public const string LOGGING_AND_PREP = "1";
            public const string APPROVAL = "2";
            public const string IMPLEMENTING = "3";
            public const string CLOSED = "4";
        }
        public static class DataElementDetailCodes
        {
            public const string CREATE_CURRENT_VIEW = "CreateCurrentView";
            public const string DELETE_INDICATOR = "DeleteInd";
            public const string DELETE_ISSUER = "DeleteIssuer";
            public const string DELETE_ISSUE_DTM = "DeleteIssueDTM";
            public const string INCLUDE_IN_SAS = "SAS_IND";
            public static string SAS_LIBRARY = "SASLibrary";
        }

        public static class Datatypes
        {
            public const string VARCHAR = "VARCHAR";
            public const string INTEGER = "INTEGER";
            public const string BIGINT = "BIGINT";
            public const string DECIMAL = "DECIMAL";
            public const string DATE = "DATE";
            public const string TIMESTAMP = "TIMESTAMP";
            public const string STRUCT = "STRUCT";

            public static class Defaults
            {                
                public const string DATE_DEFAULT = "yyyy-MM-dd";
                public const string TIMESTAMP_DEFAULT = DATE_DEFAULT +" HH:mm:ss";
                public const int VARCHAR_LENGTH_DEFAULT = 1000;
                public const int DECIMAL_PRECISION_DEFAULT = 9;
                public const int DECIMAL_SCALE_DEFAULT = 2;
            }
        }

        public static class SchemaDataTypes
        {
            public const string STRUCT = "STRUCT";
            public const string INTEGER = "INTEGER";
            public const string VARCHAR = "VARCHAR";
        }

        public static class SchemaTypes
        {
            public const string FILESCHEMA = "FileSchema";
        }

        public static class DataFlowTargetPrefixes
        {
            public const string DROP_LOCATION_PREFIX = "droplocation/";
            public const string TEMP_FILE_PREFIX = "temp-file/";
            public const string RAW_STORAGE_PREFIX = TEMP_FILE_PREFIX + "raw/";
            public const string RAW_QUERY_STORAGE_PREFIX = TEMP_FILE_PREFIX + "rawquery/";
            public const string S3_DROP_PREFIX = TEMP_FILE_PREFIX + "s3drop/";
            public const string PRODUCER_S3_DROP_PREFIX = TEMP_FILE_PREFIX + "producers3drop/";
            public const string SCHEMA_LOAD_PREFIX = TEMP_FILE_PREFIX + "schemaload/";
            public const string CONVERT_TO_PARQUET_PREFIX = TEMP_FILE_PREFIX + "parquet/";
            public const string SCHEMA_MAP_PREFIX = TEMP_FILE_PREFIX + "schemamap/";
            public const string UNCOMPRESS_ZIP_PREFIX = TEMP_FILE_PREFIX + "uncompresszip/";
            public const string UNCOMPRESS_GZIP_PREFIX = TEMP_FILE_PREFIX + "uncompressgzip/";
            public const string GOOGLEAPI_PREPROCESSING_PREFIX = TEMP_FILE_PREFIX + "googleapipreprocessing/";
            public const string CLAIMIQ_PREPROCESSING_PREFIX = TEMP_FILE_PREFIX + "claimiqpreprocessing/";
            public const string FIXEDWIDTH_PREPROCESSING_PREFIX = TEMP_FILE_PREFIX + "fixedwidthpreprocessing/";
        }

        public static class DataFlowStepEvent
        {
            public const string S3_DROP_START = "DATAFLOWSTEP_S3DROP_START";
            public const string PRODUCER_S3_DROP_START = "DATAFLOWSTEP_PRODUCERS3DROP_START";
            public const string RAW_STORAGE_START = "DATAFLOWSTEP_RAWSTORAGE_START";
            public const string QUERY_STORAGE_START = "DATAFLOWSTEP_QUERYSTORAGE_START";
            public const string SCHEMA_LOAD_START = "DATAFLOWSTEP_SCHEMA_LOAD_START";
            public const string SCHEMA_MAP_START = "DATAFLOWSTEP_SCHEMA_MAP_START";
            public const string CONVERT_TO_PARQUET_START = "DATAFLOWSTEP_CONVERTTOPARQUET_START";
            public const string UNCOMPRESS_ZIP_START = "DATAFLOWSTEP_UNCOMPRESSZIP_START";
            public const string UNCOMPRESS_GZIP_START = "DATAFLOWSTEP_UNCOMPRESSGZIP_START";
            public const string GOOGLEAPI_PREPROCESSING_START = "DATAFLOWSTEP_GOOGLEAPIPREPROCESSING_START";
            public const string CLAIMIQ_PREPROCESSING_START = "DATAFLOWSTEP_CLAIMIQPREPROCESSING_START";
            public const string FIXEDWIDTH_PREPROCESSING_START = "DATAFLOWSTEP_FIXEDWIDTH_START";
        }

        public static class AWSEventNotifications
        {
            public static class S3Events
            {
                public const string OBJECTCREATED_PUT = "OBJECTCREATED:PUT";
                public const string OBJECTCREATED_POST= "OBJECTCREATED:POST";
                public const string OBJECTCREATED_COPY = "OBJECTCREATED:COPY";
                public const string OBJECTCREATED_COMPLETEMULTIPARTUPLOAD = "OBJECTCREATED:COMPLETEMULTIPARTUPLOAD";
            }
        }

        public static class DataFlowGuidConfiguration
        {
            public const string GUID_CULTURE = "en-US";
            public const string GUID_FORMAT = "yyyyMMddHHmmssfff";
        }

        public static class DocumentationLinks
        {
            public const string SCHEMA_ROOT_PATH_USAGE = "https://confluence.sentry.com/display/CLA/Schema+Root+Path+Option";
        }
    }
}