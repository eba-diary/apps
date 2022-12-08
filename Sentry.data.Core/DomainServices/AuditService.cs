using Sentry.data.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Sentry.data.Core
{
    public class AuditService : IAuditService
    {
        private readonly IConfigService _configService;
        private readonly ISnowProvider _snowProvider;

        public AuditService(IConfigService configService, ISnowProvider snowProvider)
        {
            _configService = configService;
            _snowProvider = snowProvider;
        }

        public BaseAuditDto GetNonParquetFiles(int datasetId, int schemaId, string queryParameter, AuditSearchType auditSearchType)
        {
            // Create snowflake schema object
            SchemaConsumptionSnowflakeDto schemaObject = GetSchemaObjectBySchemaId(datasetId, schemaId);

            BaseAuditDto baseAuditDto = new BaseAuditDto();

            if (schemaObject != null) {

                //create config object
                SnowCompareConfig snowCompareConfig = CreateConfigObject(schemaObject, queryParameter, auditSearchType);

                CheckIfTableExists(snowCompareConfig.TargetDb, snowCompareConfig.Schema, snowCompareConfig.Table);

                DataTable dataTable = InvokeAuditMethod(_snowProvider.GetNonParquetFiles, snowCompareConfig);

                baseAuditDto = dataTable.NonParquetMapping();   
            }

            return baseAuditDto;
        }

        public BaseAuditDto GetComparedRowCount(int datasetId, int schemaId, string queryParameter, AuditSearchType auditSearchType)
        {
            // Create snowflake schema object
            SchemaConsumptionSnowflakeDto schemaObject = GetSchemaObjectBySchemaId(datasetId, schemaId);

            BaseAuditDto baseAuditDto = new BaseAuditDto();

            if (schemaObject != null)
            {

                SnowCompareConfig snowCompareConfig = CreateConfigObject(schemaObject, queryParameter, auditSearchType);

                CheckIfTableExists(snowCompareConfig.TargetDb, snowCompareConfig.Schema, snowCompareConfig.Table);

                DataTable dataTable = InvokeAuditMethod(_snowProvider.GetComparedRowCount, snowCompareConfig);

                baseAuditDto = dataTable.ComparedRowCountMapping();
            }

            return baseAuditDto;
        }

        private DataTable InvokeAuditMethod(Func<SnowCompareConfig,DataTable> auditMethod, SnowCompareConfig snowCompareConfig)
        {
            try
            {
                //invokes passed in audit method to query snowflake and return DataTable with resulting data
                return auditMethod(snowCompareConfig);

            }
            catch (Exception ex)
            {
                //returns descriptive error message, regardless if expection has inner exception or not
                var errorMessage = (ex.InnerException != null) ? ex.InnerException.Message : ex.Message;

                throw new ArgumentException(errorMessage);
            }
        }

        private void CheckIfTableExists(string db, string schema, string table)
        {
            try
            {
                _snowProvider.CheckIfExists(db, schema, table);
            }
            catch (Exception)
            {
                throw new ArgumentException($"The table ({table}) trying to be compared does not exist in the current context.");
            }
        }

        private SnowCompareConfig CreateConfigObject(SchemaConsumptionSnowflakeDto schemaObject, string queryParameter, AuditSearchType auditSearchType)
        {
            SnowCompareConfig snowCompareConfig = new SnowCompareConfig()
            {
                SourceDb = FindRawQueryDBName(schemaObject.SnowflakeDatabase),
                TargetDb = schemaObject.SnowflakeDatabase,
                Schema = schemaObject.SnowflakeSchema,
                Table = schemaObject.SnowflakeTable,
                QueryParameter = queryParameter,
                AuditSearchType = auditSearchType,
            };

            return snowCompareConfig;
        }

        private SchemaConsumptionSnowflakeDto GetSchemaObjectBySchemaId(int datasetId, int schemaId)
        {
            //find DatasetFileConfigDto by parameter datasetId + schemaId 
            DatasetFileConfigDto datasetFileConfigDto = _configService.GetDatasetFileConfigDtoByDataset(datasetId).FirstOrDefault(w => w.Schema.SchemaId == schemaId);

            //find SchemaConsumptionSnowflakeDto with snowflake consumption of dataset schema parquet
            SchemaConsumptionSnowflakeDto schemaObject = datasetFileConfigDto.Schema.ConsumptionDetails.OfType<SchemaConsumptionSnowflakeDto>()
                                                            .FirstOrDefault(x => x.SnowflakeType == SnowflakeConsumptionType.CategorySchemaParquet);

            //if no object is found with a snowflake consumption of dataset schema parquet, set to first or default
            if (schemaObject is null)
            {
                schemaObject = datasetFileConfigDto.Schema.ConsumptionDetails.OfType<SchemaConsumptionSnowflakeDto>().FirstOrDefault();
            }

            return schemaObject;
        }

        //derive the rawquery database name from the passed in database name
        private string FindRawQueryDBName(string db)
        {
            //derive rawquery db name from passerd in snowflake database name.
            string rawqueryDB = db.Replace("_", "_RAWQUERY_");

            return rawqueryDB;
        }
    }
}