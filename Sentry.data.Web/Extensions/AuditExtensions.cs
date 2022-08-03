using Sentry.Common.Logging;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Exceptions;
using Sentry.data.Web;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public static class AuditExtensions
    {

        public static BaseAuditModel MapToCompareModel(this BaseAuditDto baseAudit)
        {
            BaseAuditModel model = new BaseAuditModel(){  DataFlowStepId = baseAudit.DataFlowStepId, DatafileModels = new List<AuditDataFileModel>() };

            foreach (CompareAuditDto compareAuditDto in baseAudit.AuditDtos)
            {
                model.DatafileModels.Add(new CompareAuditDataFileModel()
                {
                    DatasetFileId = compareAuditDto.DatasetFileId,
                    DatasetFileName = compareAuditDto.DatasetFileName,
                    ParquetRowCount = compareAuditDto.ParquetRowCount,
                    RawqueryRowCount = compareAuditDto.RawqueryRowCount
                });
            }

            return model;
        }

        public static BaseAuditModel MapToModel(this BaseAuditDto baseAudit)
        {
            BaseAuditModel model = new BaseAuditModel() { DataFlowStepId = baseAudit.DataFlowStepId, DatafileModels = new List<AuditDataFileModel>() };

            foreach (AuditDto compareAuditDto in baseAudit.AuditDtos)
            {
                model.DatafileModels.Add(new AuditDataFileModel()
                {
                    DatasetFileId = compareAuditDto.DatasetFileId,
                    DatasetFileName = compareAuditDto.DatasetFileName
                });
            }

            return model;
        }
    }
}
