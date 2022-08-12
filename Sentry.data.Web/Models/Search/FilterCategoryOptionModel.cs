using System;
using System.Web;

namespace Sentry.data.Web
{
    public class FilterCategoryOptionModel
    {
        public string OptionId
        {
            get 
            {
                string id = HttpUtility.UrlEncode(OptionValue);

                if (!string.IsNullOrEmpty(ParentCategoryName))
                {
                    id = HttpUtility.UrlEncode(ParentCategoryName) + "_" + id;
                }

                return id;
            }
        }

        public string OptionValue { get; set; }
        public string ParentCategoryName { get; set; }
        public long ResultCount { get; set; }
        public bool Selected { get; set; }
    }
}