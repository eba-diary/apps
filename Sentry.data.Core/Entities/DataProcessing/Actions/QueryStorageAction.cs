namespace Sentry.data.Core.Entities.DataProcessing
{
    public class QueryStorageAction : BaseAction
    {
        public QueryStorageAction()
        {
            TargetStoragePrefix = GlobalConstants.DataFlowTargetPrefixes.RAW_QUERY_STORAGE_PREFIX;
        }
    }
}
