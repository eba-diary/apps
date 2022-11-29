namespace Sentry.data.Core
{
    public class DataSourceType
    {
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual string DiscrimatorValue { get; set;}
    }
}
