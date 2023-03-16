using Sentry.data.Core.GlobalEnums;
using System;

namespace Sentry.data.Core
{
    public class BaseResultDto
    {
        public ObjectStatusEnum ObjectStatus { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime UpdateDateTime { get; set; }
    }
}
