using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class FilterCategoryDto
    {
        public string CategoryName { get; set; }
        public List<string> CategoryOptionValues { get; set; }
    }
}
