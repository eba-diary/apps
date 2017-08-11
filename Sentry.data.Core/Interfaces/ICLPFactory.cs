using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface ICLPFactory
    {
        IConsumptionLayerProvider GetCLPFor(ConsumptionLayerComponent clc);
    }
}
