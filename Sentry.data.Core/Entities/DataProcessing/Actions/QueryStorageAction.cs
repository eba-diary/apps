namespace Sentry.data.Core.Entities.DataProcessing
{
    public class QueryStorageAction : BaseAction
    {
        public QueryStorageAction()
        {
            TargetStoragePrefix = "rawquery/";
        }
    }
}
