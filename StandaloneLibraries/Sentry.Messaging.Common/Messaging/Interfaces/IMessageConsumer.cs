namespace Sentry.Messaging.Common
{
    public delegate void OnMessageReadyHandler<in msgT>(object sender, msgT msg);
    public delegate void OnConsumerStoppedHandler(object sender, bool success);
    public delegate void OnEndOfStreamHandler(object sender);
    public delegate void OnSubscriptionReadyHandler(object sender, bool ready);

    public interface IMessageConsumer<out msgT>
    {
        event OnMessageReadyHandler<msgT> MessageReady;
        event OnConsumerStoppedHandler ConsumerStopped;
        event OnEndOfStreamHandler EndOfStream;
        event OnSubscriptionReadyHandler SubscriptionReady;

        void Open();
        void Close();
        void RequestStop();
    }
}
