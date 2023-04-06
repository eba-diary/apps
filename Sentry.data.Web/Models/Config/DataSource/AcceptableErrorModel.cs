using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Models.Config.DataSource
{
    [Serializable]
    public class AcceptableErrorModel
    {
        public virtual string Index { get; set; }
        public virtual string ErrorMessageKey { get; set; }
        public virtual string ErrorMessageValue { get; set; }
    }
}