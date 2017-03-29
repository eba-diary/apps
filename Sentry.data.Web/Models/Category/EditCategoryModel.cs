using Sentry.data.Core;
using System.ComponentModel;

namespace Sentry.data.Web
{
    public class EditCategoryModel : BaseCategoryModel
    {
        public EditCategoryModel()
        {

        }

        public EditCategoryModel(Category category) : base(category)
        {
            if (category.ParentCategory != null)
            {
                this.ParentCategoryId = category.ParentCategory.Id;
            }
        }

        public int? ParentCategoryId { get; set; }

        [DisplayName("Parent Category")]
        public string ParentCategoryName { get; set; }

    }
}
