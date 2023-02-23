using System.Collections.Concurrent;

namespace Sentry.data.Web.API
{
    public class ConcurrentFieldValidationResponse
    {
        public string Field { get; set; }
        public ConcurrentQueue<string> ValidationMessages { get; protected set; }

        public void AddValidationMessage(string message)
        {
            if (ValidationMessages == null)
            {
                ValidationMessages = new ConcurrentQueue<string>();
            }

            ValidationMessages.Enqueue(message);
        }
    }
}