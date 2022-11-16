using Sentry.Common.Logging;
using Sentry.data.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            SchemaConsumptionSnowflakeDto schemaObject = getSchemaObjectBySchemaId(datasetId, schemaId);

            BaseAuditDto baseAuditDto = new BaseAuditDto();

            if (schemaObject != null) {

                //create config object
                SnowCompareConfig snowCompareConfig = createConfigObject(schemaObject, queryParameter, auditSearchType);

                checkIfTableExists(snowCompareConfig.TargetDb, snowCompareConfig.Schema, snowCompareConfig.Table);

                //calls to the snow provider to create call snowflake and return DataTable with resulting data
                DataTable dataTable = _snowProvider.GetNonParquetFiles(snowCompareConfig);

                List<AuditDto> auditDtos = new List<AuditDto>();

                //check if the data table is not null and then map it's values to the list of Audit Dto's
                if (dataTable.Rows != null)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        auditDtos.Add(new AuditDto() {
                            DatasetFileName = DatabaseHelper.SafeDatabaseString(row["ETL_FILE_NAME"]),
                            ParquetRowCount = 0,
                            RawqueryRowCount = 0
                        });
                    }
                }

                baseAuditDto.AuditDtos = auditDtos;
            }

            return baseAuditDto;
        }

        public BaseAuditDto GetComparedRowCount(int datasetId, int schemaId, string queryParameter, AuditSearchType auditSearchType)
        {
            // Create snowflake schema object
            SchemaConsumptionSnowflakeDto schemaObject = getSchemaObjectBySchemaId(datasetId, schemaId);

            BaseAuditDto baseAuditDto = new BaseAuditDto();

            if (schemaObject != null)
            {

                SnowCompareConfig snowCompareConfig = createConfigObject(schemaObject, queryParameter, auditSearchType);

                checkIfTableExists(snowCompareConfig.TargetDb, snowCompareConfig.Schema, snowCompareConfig.Table);

                //calls to the snow provider to create call snowflake and return DataTable with resulting data
                DataTable dataTable = _snowProvider.GetComparedRowCount(snowCompareConfig);

                List<AuditDto> auditDtos = new List<AuditDto>();

                //check if the data table is not null and then map it's values to the list of Audit Dto's
                if (dataTable.Rows != null)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {       
                        auditDtos.Add(new AuditDto()
                        {
                            DatasetFileName = DatabaseHelper.SafeDatabaseString(row["ETL_FILE_NAME"]),
                            RawqueryRowCount = DatabaseHelper.SafeDatabaseInt(row["RAW_COUNT"]),
                            ParquetRowCount = DatabaseHelper.SafeDatabaseInt(row["PAR_COUNT"])
                        });

                    }
                }

                baseAuditDto.AuditDtos = auditDtos;
            }

            return baseAuditDto;
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

        private SnowCompareConfig createConfigObject(SchemaConsumptionSnowflakeDto schemaObject, string queryParameter, AuditSearchType auditSearchType)
        {
            SnowCompareConfig snowCompareConfig = new SnowCompareConfig()
            {
                SourceDb = findRawQueryDBName(schemaObject.SnowflakeDatabase),
                TargetDb = schemaObject.SnowflakeDatabase,
                Schema = schemaObject.SnowflakeSchema,
                Table = schemaObject.SnowflakeTable,
                QueryParameter = queryParameter,
                AuditSearchType = auditSearchType,
            };

            return snowCompareConfig;
        }

        private SchemaConsumptionSnowflakeDto getSchemaObjectBySchemaId(int datasetId, int schemaId)
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
        private string findRawQueryDBName(string db)
        {
            //derive rawquery db name from passerd in snowflake database name.
            string rawqueryDB = db.Replace("_", "_RAWQUERY_");

            return rawqueryDB;
        }
    }
}