namespace Sentry.Messaging.Common
{
    public interface IMessageHandler<in T>
    {
        void Init();
        void Handle(T msg);
        bool HandleComplete();
    }
}
