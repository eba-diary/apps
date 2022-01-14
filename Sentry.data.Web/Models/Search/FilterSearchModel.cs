﻿using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class FilterSearchModel
    {
        public string SearchText { get; set; }
        public List<FilterCategoryModel> FilterCategories { get; set; }
        public string IconPath { get; set; }
        public string PageTitle { get; set; }
    }
}