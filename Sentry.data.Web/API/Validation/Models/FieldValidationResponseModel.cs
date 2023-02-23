using System.Collections.Generic;

namespace Sentry.data.Web.API
{
    public class FieldValidationResponseModel
    {
        public string Field { get; set; }
        public List<string> ValidationMessages { get; set; }
    }
}