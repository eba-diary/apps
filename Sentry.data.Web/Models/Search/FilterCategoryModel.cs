using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class FilterCategoryModel
    {
        public string CategoryName { get; set; }
        public List<FilterCategoryOptionModel> CategoryOptions { get; set; }
    }
}