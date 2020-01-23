using Sentry.data.Core.Interfaces.DataProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class UncompressZipAction : BaseAction
    {
        public UncompressZipAction() { }

        public UncompressZipAction(IUncompressZipProvider uncompressZipProvider) : base(uncompressZipProvider)
        {
            TargetStoragePrefix = GlobalConstants.DataFlowTargetPrefixes.CONVERT_TO_PARQUET_PREFIX;
        }
    }
}
