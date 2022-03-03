namespace Sentry.data.Core
{
    public interface IInstanceGenerator
    {
        T GenerateInstance<T>();
    }
}
