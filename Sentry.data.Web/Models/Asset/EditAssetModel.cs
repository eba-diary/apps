using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class EditAssetModel : BaseAssetModel
    {
        /// <summary>
        /// Parameterless constructor is needed for view binding
        /// </summary>
        public EditAssetModel()
        {

        }

        public EditAssetModel(Asset asset) : base(asset)
        {
            this.CategoryIDs = asset.Categories.Select(((c) => c.Id)).ToArray();
        }

        /// <summary>
        /// CategoryIDs holds the IDs of the selected categories.  
        /// It is needed for model binding and MVC editor helpers
        /// </summary>
        [DisplayName("Categories")]
        [Required]
        public int[] CategoryIDs { get; set; }

        /// <summary>
        /// AllCategories holds the sorted list of all possible categories.
        /// </summary>
        public IEnumerable<SelectListItem> AllCategories { get; set; }
    }
}
