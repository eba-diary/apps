using Sentry.data.Core.Entities.DataProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Interfaces.DataProcessing
{
    public interface IDataStep
    {
        string TargetPrefix { get; set; }
        BaseAction Action { get; set; }
    }
}
