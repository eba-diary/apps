using Sentry.data.Core;
using System;
using System.Globalization;
using System.Linq;

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

        /// <summary>
        /// Return file name suffixed with flowexecutionguid.  Retruns null when filename does not 
        /// match file format.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="flowExecutionGuid"></param>
        /// <returns></returns>
        public static string AddFlowExecutionGuidToFilename(string filename, string flowExecutionGuid)
        {
            string newFilename = null;

            //testfile.txt.json will result in ['testfile', 'txt.json']
            string[] split = filename.Split(new Char[] { '.' }, 2);

            if (split.Any() && split.Count() == 2)
            {
                newFilename = $"{split[0]}_{flowExecutionGuid}.{split[1]}";
            }
            
            return newFilename;
        }
    }
}
