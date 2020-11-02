namespace Sentry.data.Core
{
    public interface IHostSettings
    {
        string this[string key] { get; }
    }
}
