using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class BaseCategoryModel
    {
        public BaseCategoryModel()
        {

        }

        public BaseCategoryModel(Category category)
        {
            this.Id = category.Id;
            this.Name = category.Name;
            this.FullName = category.FullName;
        }

        public int Id { get; set; }

        [Required()]
        [MaxLength(255)]
        [DisplayName("Category Name")]
        public string Name { get; set; }

        public string FullName { get; set; }

    }
}
