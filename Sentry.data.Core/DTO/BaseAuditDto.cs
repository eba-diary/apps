using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class BaseAuditDto
    {
        public int DataFlowStepId { get; set; }

        public List<AuditDto> AuditDtos { get; set; }
    }
}
