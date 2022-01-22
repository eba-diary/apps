using Sentry.data.Core;
using System;

namespace Sentry.data.Web
{
    public class FilterCategoryOptionModel
    {
        private readonly string _id = Guid.NewGuid().ToString();

        public string OptionId
        {
            get => string.IsNullOrEmpty(ParentCategoryName) ? _id : ParentCategoryName.Replace(" ", "_") + "_" + _id;
        }

        public string OptionValue { get; set; }
        public string ParentCategoryName { get; set; }
        public int ResultCount { get; set; }
        public bool Selected { get; set; }
    }
}