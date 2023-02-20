using System.Collections.Generic;

namespace Sentry.data.Web.API
{
    public class FieldValidationResponseModel
    {
        public string Field { get; set; }
        public List<string> ValidationMessages { get; protected set; }

        public void AddValidationMessage(string message)
        {
            if (ValidationMessages == null)
            {
                ValidationMessages = new List<string>();
            }

            ValidationMessages.Add(message);
        }
    }
}