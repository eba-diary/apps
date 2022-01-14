using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class FilterCategoryOptionModel
    {
        public string OptionId { get; set; }
        public string OptionName { get; set; }
        public int ResultCount { get; set; }
        public bool DefaultSelected { get; set; }
    }
}