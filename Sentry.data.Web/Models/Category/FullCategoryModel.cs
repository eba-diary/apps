using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Web
{
    public class FullCategoryModel : BaseCategoryModel
    {
        public FullCategoryModel()
        {

        }

        public FullCategoryModel(Category category, Boolean populateSubCategories = true) : base(category)
        {
            if (category.ParentCategory != null)
            {
                this.ParentCategory = new FullCategoryModel(category.ParentCategory, false);
            }
            if (populateSubCategories)
            {
                this.SubCategories = category.SubCategories.Select(((s) => new FullCategoryModel(s))).ToList();
            }
        }

        public FullCategoryModel ParentCategory { get; set; }

        public IList<FullCategoryModel> SubCategories { get; set; }

        public Boolean IsLeaf
        {
            get
            {
                return (SubCategories == null || SubCategories.Count == 0);
            }
        }
    }
}
