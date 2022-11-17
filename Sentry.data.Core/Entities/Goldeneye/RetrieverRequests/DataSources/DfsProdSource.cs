namespace Sentry.data.Core
{
    public class DfsProdSource : DfsEnvironmentSource
    {
        public override string SourceType { get => GlobalConstants.DataSourceDiscriminator.DFS_PROD_SOURCE; }
    }
}
