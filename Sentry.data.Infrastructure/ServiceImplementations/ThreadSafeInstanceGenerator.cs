using Sentry.data.Core;

namespace Sentry.data.Infrastructure
{
    public class ThreadSafeInstanceGenerator : IInstanceGenerator
    {
        public T GenerateInstance<T>()
        {
            return Bootstrapper.Container.GetNestedContainer().GetInstance<T>();
        }
    }
}
