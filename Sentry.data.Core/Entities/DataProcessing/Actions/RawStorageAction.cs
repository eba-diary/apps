using Sentry.data.Core.Interfaces.DataProcessing;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class RawStorageAction : BaseAction
    {
        public RawStorageAction() { }
        public RawStorageAction(IRawStorageProvider rawStorageProvider) : base(rawStorageProvider)
        {
            TargetStoragePrefix = GlobalConstants.DataFlowTargetPrefixes.RAW_STORAGE_PREFIX + Configuration.Config.GetHostSetting("S3DataPrefix");
        }
    }
}
