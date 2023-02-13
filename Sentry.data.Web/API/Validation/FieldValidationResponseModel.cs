using System.Collections.Generic;
using System.Linq.Dynamic;

namespace Sentry.data.Web.API
{
    public class FieldValidationResponseModel
    {
        public string Field { get; set; }
        public List<string> ValidationMessages { get; set; }

        public void AddValidationMessage(string message)
        {
            if (ValidationMessages?.Any() == false)
            {
                ValidationMessages = new List<string>();
            }

            ValidationMessages.Add(message);
        }
    }
}