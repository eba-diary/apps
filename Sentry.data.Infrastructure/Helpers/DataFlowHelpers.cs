using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.Helpers
{
    public class DataFlowHelpers
    {
        protected DataFlowHelpers() { }

        public static DateTime ConvertFlowGuidToDateTime(string flowGuid)
        {
            CultureInfo provider = new CultureInfo(GlobalConstants.DataFlowGuidConfiguration.GUID_CULTURE);
            DateTime flowGuidDTM = DateTime.ParseExact(flowGuid, GlobalConstants.DataFlowGuidConfiguration.GUID_FORMAT, provider);
            return flowGuidDTM;
        }

        public static string GenerateGuid(string executionGuid, string instanceGuid)
        {
            if (instanceGuid == null)
            {
                return executionGuid;
            }
            else
            {
                return executionGuid + "-" + instanceGuid;
            }
        }
    }
}
