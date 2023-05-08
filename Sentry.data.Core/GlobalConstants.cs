using System;

namespace Sentry.data.Core
{
    public static class GlobalConstants
    {        
        public static class System
        {
            public const string NAME = "Data.Sentry.Com";
            public const string ABBREVIATED_NAME = "DSC";
            public const string REQUEST_CONTEXT_GUID_FORMAT = "yyyyMMddHHmmssfff";
        }

        public static class Environments
        {
            public const string TEST = "TEST";
            public const string QUAL = "QUAL";
            public const string PROD = "PROD";
            public const string NONPROD_SUFFIX = "NP";
        }

        public static class ValidationErrors
        {
            public const string S3KEY_IS_BLANK = "keyIsBlank";
            public const string NAME_IS_BLANK = "nameIsBlank";
            public const string NAME_IS_IDEMPOTENT = "nameIsIdempotent";
            public const string SAID_ASSET_REQUIRED = "saidAssetRequired";
            public const string SAID_ASSET_IDEMPOTENT = "saidAssetIdempotent";
            public const string NAMED_ENVIRONMENT_INVALID = "namedEnvironmentInvalid";
            public const string NAMED_ENVIRONMENT_IDEMPOTENT = "namedEnvironmentIdempotent";
            public const string NAMED_ENVIRONMENT_TYPE_INVALID = "namedEnvironmentTypeInvalid";
            public const string NAMED_ENVIRONMENT_TYPE_IDEMPOTENT = "namedEnvironmentTypeIdempotent";
        }


        public static class DataEntityCodes
        {
            public const string REPORT = "RPT";
            public const string DATASET = "DS"; 
        }

        public static class EventType
        {
            public const string CREATED_FILE = "Created File";
            public const string VIEWED = "Viewed";
            public const string SEARCH = "Search";
            public const string DOWNLOAD = "Downloaded Data File";
            public const string EDITED_DATA_FILE = "Edited Data File";

            public const string VIEWED_REPORT = "Viewed Report";
            public const string UPDATED_REPORT = "Updated Report";
            public const string DELETED_REPORT = "Deleted Report";
            public const string DOWNLOADED_REPORT = "Downloaded Report";

            public const string VIEWED_DATASET = "Viewed Dataset";
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

            public const string DATA_INVENTORY_SEARCH = "DataInventoryQuery";

            //DSC NOTIFICATION FEED EVENTS
            public const string CREATED_REPORT = "Created Report";
            public const string CREATED_DATASET = "Created Dataset";
            public const string CREATE_DATASET_SCHEMA = "Created Dataset Schema";
            public const string NOTIFICATION_DSC_RELEASE_NOTES = "Release Notes";
            public const string NOTIFICATION_DSC_TECH_DOC = "Technical Documentation";
            public const string NOTIFICATION_DSC_NEWS = "News";

            public const string NOTIFICATION_DSC_RELEASENOTES_DSC = "DSC";
            public const string NOTIFICATION_DSC_RELEASENOTES_CL = "CL";
            public const string NOTIFICATION_DSC_RELEASENOTES_PL = "PL";
            public const string NOTIFICATION_DSC_RELEASENOTES_LIFEANNUITY = "LifeAnnuity";
            public const string NOTIFICATION_DSC_RELEASENOTES_CLAIMS = "Claims";
            public const string NOTIFICATION_DSC_RELEASENOTES_CORPORATE = "Corporate";

            public const string NOTIFICATION_DSC_NEWS_DSC = "DSC";
            public const string NOTIFICATION_DSC_NEWS_TABLEAU = "Tableau";
            public const string NOTIFICATION_DSC_NEWS_PYTHON = "Python";
            public const string NOTIFICATION_DSC_NEWS_SAS = "SAS";
            public const string NOTIFICATION_DSC_NEWS_ANALYTICS = "Analytics";

            //EVENT TYPES FOR DELETE S3 RELATED
            public const string DATASETFILE_DELETE_S3 = "DatasetFileDeleteS3";
            public const string DATASETFILE_UPDATE_OBJECT_STATUS = "DatasetFileUpdateObjectStatus";
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

        public static class DataSourceDiscriminator
        {
            public const string DFS_SOURCE = "DFS";
            public const string DFS_NONPROD_SOURCE = "DFSNonProd";
            public const string DFS_PROD_SOURCE = "DFSProd";
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
            public const string GOOGLE_BIG_QUERY_API_SOURCE = "GoogleBigQueryApi";
            public const string PAGING_HTTPS_SOURCE = "PagingHttps";
            public const string GOOGLE_SEARCH_CONSOLE_API_SOURCE = "GoogleSearchConsoleApi";
        }

        public static class DataFeedType
        {
            public const string SAS = "SAS";
            public const string Tab = "TAB";
            public const string Datasets = "Datasets";
            public const string DataAssets = "Data Assets";
            public const string Exhibits = "Exhibits";
            public const string Notifications = "Notifications";
            public const string Schemas = "Schemas";
        }


        public static class DataFeedName
        {
            public const string BUSINESS_INTELLIGENCE = "Business Intelligence";
            public const string DATASET = "Dataset";
            public const string SCHEMA = "Schema";
            public const string NOTIFICATION = "Notification";
        }

        public static class BusinessObjectExhibit
        {
            public const string GET_LATEST_URL_PARAMETER = "&sInstance=Last";
        }

        public static class ChangeTicketStatus
        {
            //DSC
            public const string PENDING = "Pending";
            public const string COMPLETED = "Completed";
            public const string DENIED = "Denied";
            public const string WITHDRAWN = "Withdrawn";
            public const string APPROVED = "approved";
            //dba
            public const string DbaTicketPending = "DbaTicketPending";
            public const string DbaTicketAdded = "DbaTicketAdded";
            public const string DbaTicketApproved = "DbaTicketApproved";
            public const string DbaTicketComplete = "DbaTicketComplete";
        }

        public static class JsmAssignmentGroup
        {
            public const string BI_PORTAL_ADMIN = "BI Portal Administration";
        }

        public static class JsmChangeStatus
        {
            public const string REVIEW = "Review";
            public const string INDIVIDUAL_AUTHORIZE = "Individual Authorize";
            public const string GROUP_AUTHORIZE = "Group Authorize";
            public const string AWAITING_IMPLEMENTATION = "Awaiting implementation";
            public const string IMPLEMENTING = "Implementing";
            public const string COMPLETED = "Completed";
            public const string CANCELED = "Canceled";
            public const string DECLINED = "Declined";
            public const string FAILED = "Failed";
        }

        public static class JsmChangePhase
        {
            public const string READY_APPROVAL = "Ready for Individual Approval";
        }

        public static class SecurableEntityName
        {
            public const string DATASET = "Dataset";
            public const string DATA_ASSET = "DataAsset";
            public const string DATASOURCE = "DataSource";
            public const string BUSINESSAREA = "BusinessArea"; 
            public const string ASSET = "Asset";
            public const string DATAFLOW = "Dataflow";
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
            public const string CAN_MANAGE_DATAFLOW = "CanManageDataflow";
            public const string S3_ACCESS = "S3Access";
            public const string SNOWFLAKE_ACCESS = "SnowflakeAccess";
            public const string INHERIT_PARENT_PERMISSIONS = "InheritParentPermissions";

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

        public static class IdentityType
        {
            public const string AD = "AD";
            public const string AWS_IAM = "AWS_IAM";
            public const string SNOWFLAKE = "SNOWFLAKE";
        }

        public static class ConvertedFileStoragePrefix
        {
            public const string PARQUET_STORAGE_PREFIX = "parquet";
        }

        public static class SearchType
        {
            public const string BUSINESS_INTELLIGENCE_SEARCH = "BusinessIntelligence";
            public const string DATASET_SEARCH = "Datasets";
            public const string DATA_INVENTORY = "DataInventory";
            public const string GLOBAL_DATASET = "GlobalDataset";
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
            public const string WAITING_FOR_APPROVAL = "Approval";
            public const string IMPLEMENTING = "Implementing";
            public const string CLOSED = "Closed";
        }

        public static class CherwellChangeStatusOrder
        {
            public const string LOGGING_AND_PREP = "1";
            public const string WAITING_FOR_APPROVAL = "2";
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
            public const string SAS_LIBRARY = "SASLibrary";
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
            public const string VARIANT = "VARIANT";

            public static class Defaults
            {                
                public const string DATE_DEFAULT = "yyyy-MM-dd";
                public const string TIMESTAMP_DEFAULT = DATE_DEFAULT +" HH:mm:ss";
                public const int VARCHAR_LENGTH_DEFAULT = 1000;
                public const int DECIMAL_PRECISION_DEFAULT = 9;
                public const int DECIMAL_SCALE_DEFAULT = 2;
                public const int LENGTH_DEFAULT = 0;
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

        public static class AwsBuckets
        {
            public const string HR_DATASET_BUCKET_AE2 = "sentry-<saidkeycode>-<namedenvironment>-hrdataset-ae2";
            public const string BASE_DATASET_BUCKET_AE2 = "sentry-<saidkeycode>-<namedenvironment>-dataset-ae2";
        }

        public static class SaidAsset
        {
            public const string DATA_LAKE_STORAGE = "DLST";
            public const string DSC = "DATA";
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

        public static class SnowflakeStageNames
        {
            public const string DATASET_STAGE = "SENTRY_DATASET";       //OLD STAGE NAME
            public const string PARQUET_STAGE = "DLST_PARQUET";         //NEW STAGE NAME
            public const string RAWQUERY_STAGE = "DLST_RAWQUERY";
            public const string RAW_STAGE = "DLST_RAW";
        }

        public static class SnowflakeWarehouse
        {
            public const string WAREHOUSE_NAME = "DATA_WH";       
        }

        public static class SnowflakeDatabase
        {
            public const string WDAY = "WDAY_";
            public const string DATA = "DATA_";
        }

        public static class SnowflakeConsumptionLayerPrefixes
        {
            public const string RAW_PREFIX = "RAW_";
            public const string RAWQUERY_PREFIX = "RAWQUERY_";
        }

        public static class ElasticAliases
        {
            public const string DATA_INVENTORY = "data-inventory";
        }

        public static class FilterCategoryNames
        {
            public static class DataInventory
            {
                public const string ASSET = "Asset";
                public const string COLLECTIONNAME = "Collection Name";
                public const string COLUMN = "Column";
                public const string DATATYPE = "Datatype";
                public const string DATABASE = "Database";
                public const string NULLABLE = "Nullable";
                public const string SENSITIVE = "Sensitive";
                public const string ENVIRONMENT = "Environment";
                public const string SERVER = "Server";
                public const string SOURCETYPE = "Source Type";
                public const string COLLECTIONTYPE = "Collection Type";
            }

            public static class Dataset
            {
                public const string FAVORITE = "Favorite";
                public const string CATEGORY = "Category";
                public const string SECURED = "Secured";
                public const string ORIGIN = "Origin";
                public const string ENVIRONMENT = "Environment";
                public const string ENVIRONMENTTYPE = "Environment Type";
                public const string DATASETASSET = "Dataset Asset";
                public const string PRODUCERASSET = "Producer Asset";
            }

            public static class BusinessIntelligence
            {
                public const string REPORTTYPE = "Report Type";
                public const string BUSINESSUNIT = "Business Unit";
                public const string FUNCTION = "Function";
                public const string TAG = "Tag";
            }
        }

        public static class SearchDisplayNames
        {
            public static class GlobalDataset
            {
                public const string DATASETNAME = "Dataset Name";
                public const string DATASETDESCRIPTION = "Dataset Description";
                public const string SCHEMANAME = "Schema Name";
                public const string SCHEMADESCRIPTION = "Schema Description";
            }
        }

        public static class FilterCategoryOptions
        {
            public const string ENVIRONMENT_PROD = "P";
            public const string ENVIRONMENT_NONPROD = "D";
        }

        public static class UserFavoriteTypes
        {
            public const string SAVEDSEARCH = "SavedSearch";
        }

        public static class SaveSearchResults
        {
            public const string NEW = "New";
            public const string EXISTS = "Exists";
            public const string UPDATE = "Update";
        }

        public static class DataInventorySearchTargets
        {
            public const string SAID = "SAID";
            public const string SERVER = "SERVER";
        }

        public static class ExtensionNames
        {
            public const string FIXEDWIDTH = "FIXEDWIDTH";
            public const string XML = "XML";
            public const string JSON = "JSON";
            public const string DELIMITED = "DELIMITED";
            [Obsolete("Not supported for new Schema")]
            public const string ANY = "ANY"; //deprecated
            public const string CSV = "CSV";
            public const string TXT = "TXT";
            [Obsolete("Format not supported")]
            public const string XLSX = "XLSX"; //deprecated
            public const string PARQUET = "PARQUET";
        }

        public static class DeleteFileResponseStatus
        {
            public const string SUCCESS = "SUCCESS";
            public const string FAILURE = "FAILURE";
            public const string NOTFOUND = "NOTFOUND";
            public const string ERROR = "ERROR";
        }

        public static class DeleteFileResponseFileType
        {
            public const string PARQUET = "PARQUET";
          
        }

        public static class SecurityConstants
        {
            public const string ASSET_LEVEL_GROUP_NAME = "Default";
        }

        public static class Pagination
        {
            public const string ELLIPSIS = "...";
        }

        public static class TileResultParameters
        {
            public const string SORTBY = "SortBy";
            public const string PAGENUMBER = "PageNumber";
            public const string PAGESIZE = "PageSize";
            public const string LAYOUT = "Layout";
        }

        public static class ExecutionParameterKeys
        {
            public static class GoogleBigQueryApi
            {
                public const string LASTINDEX = "LastIndex";
                public const string TOTALROWS = "TotalRows";
            }

            public static class PagingHttps
            {
                public const string CURRENTDATASOURCETOKENID = "CurrentDataSourceTokenId";
            }
        }

        public static class HostSettings
        {
            public const string SMTPCLIENT = "SmtpClient";
            public const string DATASETEMAIL = "DatasetMgmtEmail";
            public const string S3SINKEMAILTO = "S3SinkEmailRequestTO";
            public const string MOTIVEEMAILTO = "MotiveEmailTo";
            public const string MAIN_WEB_URL = "SentryDataBaseUrl";
            public const string CONFLUENT_CONNECTOR_API = "ConfluentConnectorApi";
            public const string CONFLUENT_CONNECTOR_USERNAME = "ConfluentConnectorUserName";
            public const string CONFLUENT_CONNECTOR_PASSWORD = "ConfluentConnectorPassword";
            public const string CONFLUENT_CONNECTOR_FLUSH_SIZE = "ConfluentConnectorFlushSize";
            public const string S3CONNECTOR_PROXY = "EdgeWebProxyUrl";
        }

        public static class DLPPEnvironments
        {
            public const string TEST = "TEST";
            public const string NRTEST = "NRTEST";
            public const string QUALNP = "QUALNP";
            public const string QUAL = "QUAL";
            public const string PRODNP = "PRODNP";
            public const string PROD = "PROD";
        }

        public static class DfsRetrieverJobProviderTypes
        {
            public const string ALL = "ALL";
            public const string QUAL = "QUAL";
            public const string PROD = "PROD";
        }

        public static class Indicators
        {
            public const string ENCRYPTIONINDICATOR = "<--!-->";
            public const string REQUESTVARIABLEINDICATOR = "~[{0}]~";
        }


        public static class MigrationHistory
        {
            public const string TYPE_DATASET = "Dataset";
            public const string TYPE_SCHEMA = "Schema";
        }

        public static class SAIDRoles
        {
            public const string CUSTODIAN_PRODUCTION = "Custodian - Production";
            public const string CUSTODIAN_CERTIFIER = "Custodian - Certifier";
        }

        public static class MigrationHistoryNamedEnvFilter
        {
            public const string ALL_NAMED_ENV = "ALL";
        }
    }
}
