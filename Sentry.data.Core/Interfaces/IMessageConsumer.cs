namespace Sentry.data.Core
{
    public interface IMessageConsumer
    {
        void Close();
        void Open();
        void RequestStop();
    }
}