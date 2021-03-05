using System.Threading.Tasks;

namespace Sentry.Messaging.Common
{
    public interface IMessageHandler<in T>
    {
        void Init();
        void Handle(T msg);
        Task HandleAsync(T msg);
        bool HandleComplete();
    }
}
