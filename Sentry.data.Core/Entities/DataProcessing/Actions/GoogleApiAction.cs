using Sentry.data.Core.Interfaces.DataProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class GoogleApiAction : BaseAction
    {
        public GoogleApiAction() { }
        public GoogleApiAction(IGoogleApiActionProvider googleApiProvider) : base(googleApiProvider)
        {
            TargetStoragePrefix = GlobalConstants.DataFlowTargetPrefixes.GOOGLEAPI_PREPROCESSING_PREFIX + Configuration.Config.GetHostSetting("S3DataPrefix");
        }
    }
}
