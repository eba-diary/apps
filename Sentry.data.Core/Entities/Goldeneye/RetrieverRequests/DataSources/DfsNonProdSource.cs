using Sentry.Core;

namespace Sentry.data.Core
{
    public class DfsNonProdSource : DfsEnvironmentSource
    {      
        public override string SourceType { get => GlobalConstants.DataSourceDiscriminator.DFS_NONPROD_SOURCE; }
    }
}
