using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class ValidationResultViewModel
    {
        public string InvalidField { get; set; }
        public List<string> ValidationMessages { get; set; }
    }
}