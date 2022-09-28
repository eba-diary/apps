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

        public BaseAuditDto GetExceptRows(int datasetId, int schemaId, string queryParameter, AuditSearchType auditSearchType)
        {
            SchemaConsumptionSnowflakeDto schemaObject = GetSchemaObjectBySchemaId(datasetId, schemaId);

            BaseAuditDto baseAuditDto = new BaseAuditDto();

            if (schemaObject != null) {

                //query execution logger
                Logger.Info("Audit Query Execution: Started");
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                //create config object
                SnowCompareConfig snowCompareConfig = createConfigObject(schemaObject, queryParameter, auditSearchType);

                if (!_snowProvider.CheckIfExists(snowCompareConfig.TargetDb, snowCompareConfig.Schema, snowCompareConfig.Table))
                {
                    throw new ArgumentException($"The table ({snowCompareConfig.Table}) trying to be compared does not exist in the current context.");
                }

                //calls to the snow provider to create call snowflake and return DataTable with resulting data
                DataTable dataTable = _snowProvider.GetExceptRows(snowCompareConfig);

                stopWatch.Stop();

                TimeSpan ts = stopWatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);

                Logger.Info("Audit Query Execution: Complete");
                Logger.Info($"Elapsed Time: {elapsedTime}");

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

        public BaseAuditDto GetRowCountCompare(int datasetId, int schemaId, string queryParameter, AuditSearchType auditSearchType)
        {
            SchemaConsumptionSnowflakeDto schemaObject = GetSchemaObjectBySchemaId(datasetId, schemaId);

            BaseAuditDto baseAuditDto = new BaseAuditDto();

            if (schemaObject != null)
            {
                Logger.Info("Audit Query Execution: Started");
                Stopwatch stopWatch = new Stopwatch();

                stopWatch.Start();

                SnowCompareConfig snowCompareConfig = createConfigObject(schemaObject, queryParameter, auditSearchType);

                if(!_snowProvider.CheckIfExists(snowCompareConfig.TargetDb, snowCompareConfig.Schema, snowCompareConfig.Table))
                {
                    throw new ArgumentException($"Table: {snowCompareConfig.Table} does not exist in the current context");
                }
                
                //calls to the snow provider to create call snowflake and return DataTable with resulting data
                DataTable dataTable = _snowProvider.GetCompareRows(snowCompareConfig);

                stopWatch.Stop();

                TimeSpan ts = stopWatch.Elapsed;

                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);

                Logger.Info("Audit Query Execution: Complete");
                Logger.Info($"Elapsed Time: {elapsedTime}");


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

        private SchemaConsumptionSnowflakeDto GetSchemaObjectBySchemaId(int datasetId, int schemaId)
        {
            //find DatasetFileConfigDto by parameter datasetId + schemaId 
            DatasetFileConfigDto datasetFileConfigDto = _configService.GetDatasetFileConfigDtoByDataset(datasetId).FirstOrDefault(w => w.Schema.SchemaId == schemaId);

            //find SchemaConsumptionSnowflakeDto with snowflake consumption of dataset schema parquet
            SchemaConsumptionSnowflakeDto schemaObject = datasetFileConfigDto.Schema.ConsumptionDetails.OfType<SchemaConsumptionSnowflakeDto>()
                                                            .FirstOrDefault(x => x.SnowflakeType == SnowflakeConsumptionType.DatasetSchemaParquet);

            //if no object is found with a snowflake consumption of dataset schema parquet, set to first or default
            if (schemaObject is null)
            {
                schemaObject = datasetFileConfigDto.Schema.ConsumptionDetails.OfType<SchemaConsumptionSnowflakeDto>().FirstOrDefault();
            }

            return schemaObject;
        }

        private string findRawQueryDBName(string db)
        {
            //derive rawquery db name from passerd in snowflake database name.
            string rawqueryDB = db.Replace("_", "_RAWQUERY_");

            return rawqueryDB;
        }
    }
}