using Sentry.data.Core;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Web
{
    public class FilterCategoryModel
    {
        public string CategoryName { get; set; }
        public List<FilterCategoryOptionModel> CategoryOptions { get; set; }

        public FilterCategoryDto ToDto()
        {
            return new FilterCategoryDto()
            {
                CategoryName = CategoryName,
                CategoryOptionValues = CategoryOptions.Where(x => x.Selected).Select(x => x.OptionValue).ToList()
            };
        }
    }
}