﻿namespace Sentry.data.Core
{
    public class FilterCategoryOptionDto
    {
        public string OptionValue { get; set; }
        public long ResultCount { get; set; }
        public string ParentCategoryName { get; set; }
        public bool Selected { get; set; }
    }
}