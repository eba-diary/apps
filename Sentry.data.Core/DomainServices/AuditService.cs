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

                // Query execution logger
                Logger.Info("Audit Query Execution: Started");
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                // Create config object
                SnowCompareConfig snowCompareConfig = createConfigObject(schemaObject, queryParameter, auditSearchType);

                // Calls to the snow provider to create call snowflake and return DataTable with resulting data
                DataTable dataTable = _snowProvider.GetExceptRows(snowCompareConfig);

                stopWatch.Stop();

                TimeSpan ts = stopWatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);

                Logger.Info("Audit Query Execution: Complete");
                Logger.Info($"Elapsed Time: {elapsedTime}");

                List<AuditDto> auditDtos = new List<AuditDto>();

                // Check if the data table is not null and then map it's values to the list of Audit Dto's
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

                // Calls to the snow provider to create call snowflake and return DataTable with resulting data
                DataTable dataTable = _snowProvider.GetCompareRows(snowCompareConfig);

                stopWatch.Stop();

                TimeSpan ts = stopWatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);

                Logger.Info("Audit Query Execution: Complete");
                Logger.Info($"Elapsed Time: {elapsedTime}");


                List<AuditDto> auditDtos = new List<AuditDto>();

                // Check if the data table is not null and then map it's values to the list of Audit Dto's
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
            DatasetFileConfigDto datasetFileConfigDto = _configService.GetDatasetFileConfigDtoByDataset(datasetId).FirstOrDefault(w => w.Schema.SchemaId == schemaId);

            SchemaConsumptionSnowflakeDto schemaObject = null;

            schemaObject = datasetFileConfigDto.Schema.ConsumptionDetails.OfType<SchemaConsumptionSnowflakeDto>()
                            .Where(x => x.SnowflakeType == SnowflakeConsumptionType.DatasetSchemaParquet)
                            .DefaultIfEmpty(datasetFileConfigDto.Schema.ConsumptionDetails.OfType<SchemaConsumptionSnowflakeDto>().FirstOrDefault())
                            .First();

            return schemaObject;
        }

        private string findRawQueryDBName(string db)
        {
            string rawqueryDB = db.Replace("_", "_RAWQUERY_");

            return rawqueryDB;
        }
    }
}