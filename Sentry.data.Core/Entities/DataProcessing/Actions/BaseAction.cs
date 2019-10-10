using System;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public abstract class BaseAction
    {
        public virtual int Id { get; set; }
        public virtual Guid ActionGuid { get; set; }
        public virtual string Name { get; set; }
        public virtual string TargetStoragePrefix { get; set; }
        public virtual string TargetStorageBucket { get; set; }
    }
}
