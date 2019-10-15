namespace Sentry.data.Core.Entities.DataProcessing
{
    public class RawStorageAction : BaseAction
    {
        public RawStorageAction()
        {
            TargetStoragePrefix = GlobalConstants.DataFlowTargetPrefixes.RAW_STORAGE_PREFIX;
        }
    }
}
