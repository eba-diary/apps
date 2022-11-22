using Sentry.data.Core;
using System.Collections.Generic;

namespace Sentry.data.Web
{
    public static class AuditExtensions
    {
        public static BaseAuditModel MapToModel(this BaseAuditDto baseAudit)
        {
            BaseAuditModel model = new BaseAuditModel() { DatafileModels = new List<AuditDataFileModel>() };

            foreach (AuditDto compareAuditDto in baseAudit.AuditDtos)
            {
                model.DatafileModels.Add(new AuditDataFileModel()
                {
                    DatasetFileName = compareAuditDto.DatasetFileName,
                    ParquetRowCount = compareAuditDto.ParquetRowCount,
                    RawqueryRowCount = compareAuditDto.RawqueryRowCount
                });
            }

            return model;
        }
    }
}
