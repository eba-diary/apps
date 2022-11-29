using Sentry.data.Core.Helpers;
using System.Collections.Generic;
using System.Data;

namespace Sentry.data.Core
{
    public static class AuditExtensions
    {
        public static BaseAuditDto ComparedRowCountMapping(this DataTable dataTable)
        {
            BaseAuditDto baseAuditDto = new BaseAuditDto();

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

            return baseAuditDto;
        }

        public static BaseAuditDto NonParquetMapping(this DataTable dataTable)
        {
            BaseAuditDto baseAuditDto = new BaseAuditDto();

            List<AuditDto> auditDtos = new List<AuditDto>();

            //check if the data table is not null and then map it's values to the list of Audit Dto's
            if (dataTable.Rows != null)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    auditDtos.Add(new AuditDto()
                    {
                        DatasetFileName = DatabaseHelper.SafeDatabaseString(row["ETL_FILE_NAME"]),
                        ParquetRowCount = 0,
                        RawqueryRowCount = 0
                    });
                }
            }

            baseAuditDto.AuditDtos = auditDtos;

            return baseAuditDto;
        }
    }
}