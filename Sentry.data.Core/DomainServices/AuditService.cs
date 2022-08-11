using Sentry.data.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class AuditService : IAuditService
    {
        private readonly IConfigService _configService;
        private readonly ISnowProvider _snowProvider;
        private readonly IDataFlowService _dataFlowService;

        public AuditService(IConfigService configService, ISnowProvider snowProvider, IDataFlowService dataFlowService)
        {
            _configService = configService;
            _snowProvider = snowProvider;
            _dataFlowService = dataFlowService;
        }

        public BaseAuditDto GetExceptRows(int datasetId, int schemaId, string queryParameter, AuditSearchType auditSearchType)
        {
            DatasetFileConfigDto datasetFileConfigDto = _configService.GetDatasetFileConfigDtoByDataset(datasetId).FirstOrDefault(w => w.Schema.SchemaId == schemaId);

            SchemaConsumptionSnowflakeDto schemaObject = null;

            foreach (var consumptionDetailDto in datasetFileConfigDto.Schema.ConsumptionDetails.OfType<SchemaConsumptionSnowflakeDto>())
            {
                if (consumptionDetailDto.SnowflakeType == SnowflakeConsumptionType.DatasetSchemaParquet)
                {
                    schemaObject = consumptionDetailDto;
                    break;
                }
                else
                {
                    schemaObject = consumptionDetailDto;
                }
            }

            /*DataTable dataTable = _snowProvider.GetExceptRows(schemaObject.SnowflakeDatabase, schemaObject.SnowflakeSchema, schemaObject.SnowflakeTable);*/

            // Test 
            DataTable dataTable = new DataTable();

            dataTable.Columns.Add("ETL_FILE_NAME_ONLY");
            dataTable.Rows.Add("INVOICE20210924010017_20211025015625530.xml.json");
            dataTable.Rows.Add("INVOICE007_20210616193749403.xml.json");
            dataTable.Rows.Add("INVOICE20210609082824_20220429174008000.xml.json");
            dataTable.Rows.Add("INVOICE010_20210622135943928.xml.json");
            dataTable.Rows.Add("INVOICE009_20210622123632273.xml.json");
            dataTable.Rows.Add("INVOICE011_20210622142521337.xml.json");

            BaseAuditDto baseAuditDto = new BaseAuditDto() { DataFlowStepId = 1 };

            List<AuditDto> auditDtos = new List<AuditDto>();

            int resultId = 0;

            if (dataTable.Rows != null)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    auditDtos.Add(new AuditDto() { DatasetFileId = resultId, DatasetFileName = row["ETL_FILE_NAME_ONLY"].ToString() });
                    resultId++;
                }
            }

            baseAuditDto.AuditDtos = auditDtos;

            return baseAuditDto;
        }

        public BaseAuditDto GetRowCountCompare(int datasetId, int schemaId, string queryParameter, AuditSearchType auditSearchType)
        {
            DatasetFileConfigDto datasetFileConfigDto = _configService.GetDatasetFileConfigDtoByDataset(datasetId).FirstOrDefault(w => w.Schema.SchemaId == schemaId);

            SchemaConsumptionSnowflakeDto schemaObject = null;

            foreach (var consumptionDetailDto in datasetFileConfigDto.Schema.ConsumptionDetails.OfType<SchemaConsumptionSnowflakeDto>())
            {
                if (consumptionDetailDto.SnowflakeType == SnowflakeConsumptionType.DatasetSchemaParquet)
                {
                    schemaObject = consumptionDetailDto;
                    break;
                }
                else
                {
                    schemaObject = consumptionDetailDto;
                }
            }

            /*DataTable dataTable = _snowProvider.GetExceptRows(schemaObject.SnowflakeDatabase, schemaObject.SnowflakeSchema, schemaObject.SnowflakeTable);*/

            DataTable dataTable = new DataTable();

            dataTable.Columns.Add("ETL_FILE_NAME_ONLY");
            dataTable.Columns.Add("PAR_COUNT");
            dataTable.Columns.Add("RAW_COUNT");
            dataTable.Rows.Add("INVOICE20210924010017_20211025015625530.xml.json", 45, 63);
            dataTable.Rows.Add("INVOICE007_20210616193749403.xml.json", 29,20);
            dataTable.Rows.Add("INVOICE20210609082824_20220429174008000.xml.json", 34,23);
            dataTable.Rows.Add("INVOICE010_20210622135943928.xml.json", 0,2);
            dataTable.Rows.Add("INVOICE009_20210622123632273.xml.json", 8,3);
            dataTable.Rows.Add("INVOICE011_20210622142521337.xml.json", 21,17);

            BaseAuditDto baseAuditDto = new BaseAuditDto() { DataFlowStepId = 1 };

            List<AuditDto> auditDtos = new List<AuditDto>();

            int resultId = 0;

            if (dataTable.Rows != null)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    auditDtos.Add(new CompareAuditDto() { 
                        DatasetFileId = resultId, 
                        DatasetFileName = row["ETL_FILE_NAME_ONLY"].ToString(), 
                        ParquetRowCount = DatabaseHelper.SafeDatabaseInt(row["PAR_COUNT"]), 
                        RawqueryRowCount = DatabaseHelper.SafeDatabaseInt(row["RAW_COUNT"])
                    });

                    resultId++;
                }
            }

            baseAuditDto.AuditDtos = auditDtos;

            return baseAuditDto;
        }
    }
}