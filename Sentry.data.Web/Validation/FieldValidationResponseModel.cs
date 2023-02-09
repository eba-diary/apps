using DocumentFormat.OpenXml.Office2016.Drawing.Charts;
using System.Collections.Generic;
using System.Linq.Dynamic;

namespace Sentry.data.Web
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