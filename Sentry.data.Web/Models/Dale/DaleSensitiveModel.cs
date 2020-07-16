using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class DaleSensitiveModel
    {
        public int BaseColumnId { get; set; }
        public bool SensitiveInd { get; set; }
    }
}